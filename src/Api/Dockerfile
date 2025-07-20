# This dockerfile needs to be built from the solution directory.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG QUICK=true
ARG CONFIG=Debug
WORKDIR /App
COPY ./.editorconfig ./*.sln ./
COPY ./src ./src/
COPY ./test ./test/
RUN dotnet restore
RUN "$QUICK" || dotnet format --verify-no-changes --no-restore
RUN dotnet build --no-restore
RUN "$QUICK" || dotnet test --no-restore
RUN dotnet publish ./src/Api/ -o out -c "$CONFIG" --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build /App/out .

# https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-9.0
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IdCardApi.dll"]
