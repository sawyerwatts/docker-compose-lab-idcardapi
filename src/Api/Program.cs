using System.Reflection;

using HealthChecks.UI.Client;

using IdCardApi.HealthChecks;
using IdCardApi.Middleware;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.OpenApi.Models;

using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// Non-middleware services
// -----------------------

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Services.AddSerilog();


// ----------------------------------------------------------------------------
// Middleware services
// -------------------
// Configuration should be completed at this point.

builder.Services.AddHealthChecks()
    .AddCheck<EligDbHealthCheck>("EligDb")
    .AddCheck<PlanDbHealthCheck>("PlanDb")
    .AddCheck<BlobContainerHealthCheck>("IdCardBlobContainer")
    .AddCheck<RedisHealthCheck>("Redis");

builder.Services
    .AddControllers(options =>
    {
        // If request body cannot be formatted, ASP.NET Core will automatically
        // return 415; the following enables 406 when Accept's value doesn't have a
        // formatter (because otherwise JSON is returned regardless of the Accept).
        options.ReturnHttpNotAcceptable = true;
    })
    .AddNewtonsoftJson();

builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
    })
    // Added to ensure API version is checked (else, it's weirdly hit or miss):
    .AddMvc();

TraceGuid.RegisterTo(builder);

RequestTimeouts.Add(builder);
RateLimiting.Add(builder);

// ApiKeyAuthentication.Add(builder); // TODO: add this back in

// TODO: add this back in
// Set the fallback/default authorization policy to requiring authenticated
// users. Add [AllowAnonymous] or [Authorize(PolicyName="MyPolicy")] to
// loosen/harden the authorization.
// Don't forget policies can require claims, optionally with specific values.
// builder.Services.AddAuthorization(options =>
// {
//     options.FallbackPolicy = new AuthorizationPolicyBuilder()
//         .RequireAuthenticatedUser()
//         .RequireAssertion(context =>
//             !string.IsNullOrWhiteSpace(context.User.Identity?.Name))
//         .Build();
// });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Version = "v1",
        Title = "IdCardApi",
        Description =
            """
            Note that 410: Gone is used instead of 404: Not Found when an entity does not exist at a valid URL.
            """,
    });

    string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    TraceGuid.SetupSwaggerGen(options);

    // ApiKeyAuthentication.SetupSwaggerGen(options); // TODO: add this back in
});


// ----------------------------------------------------------------------------
// Request pipeline
// ----------------
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware#middleware-order
//      Note that lots of the built in middleware need to run in a specific
//      order, so deviate from that list with caution.

WebApplication app = builder.Build();
try
{
    app.Use(TraceGuid.Middleware);
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRequestTimeouts();

    // app.UseAuthorization(); // TODO: add this back in
    app.Use(async (context, next) =>
    {
        string name = context.User.Identity?.Name ?? "`unknown`";
        string host = context.Request.Host.ToString();
        string url = context.Request.GetEncodedUrl();
        context.RequestServices.GetRequiredService<ILogger<Program>>()
            .LogInformation(
                "Request made by {User} from {Host} for {Url}",
                name,
                host,
                url);

        await next(context);
    });

    app.UseRateLimiter();

    // ----------------------------------------------------------------------------
    // Routing and startup
    // -------------------

    app.MapControllers();
    app
        .MapHealthChecks("/_health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
        })
        .AllowAnonymous();

    app.Logger.LogInformation("Instantiating app services and running");
    app.Run();
    app.Logger.LogInformation("App completed");
}
catch (Exception exc)
{
    app.Logger.LogCritical(exc, "An unhandled exception occurred, the app has crashed");
    throw;
}
finally
{
    // CA1849 wants to async-ly flush, which would end the program.
#pragma warning disable CA1849
    Log.CloseAndFlush();
#pragma warning restore CA1849
}
