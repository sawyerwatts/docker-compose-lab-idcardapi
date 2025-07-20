# This dockerfile needs to be built from the solution directory.
# NOTE: To build a deployable image, inject ARG QUICK to false, ARG CONFIG to Release, and
#       ENV ASPNETCORE_ENVIRONMENT to the server-appropriate value.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG QUICK=true
ARG CONFIG=Debug
WORKDIR /App
RUN apt-get -y update; apt-get -y install curl

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

ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "IdCardApi.dll"]
