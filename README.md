# Docker Compose Lab: ID Card API

This repository is a helper to [docker-compose-lab](https://github.com/sawyerwatts/docker-compose-lab). See that repo
for more details, but the TLDR is that this repository contains a containerized .NET API.

## Getting Started

1. Install and start Docker

    ```shell
    # Normally:
    sudo systemctl start docker

    # If rootless:
    systemctl --user start docker
    ```

1. Install .NET 8
1. Start the API's dependencies with `docker compose up idcardapi_start_dependencies -d`
1. Run this code using your IDE of choice or `dotnet run`
1. Run the tests using your IDE of choice or `dotnet test`

## Building the docker image

Recall that the following can be used to interactively play with an image.

```shell
docker run -it IMAGE
```

This is a (stubbed) API to build ID cards from the database, and cache the result.

```shell
docker compose down -v; docker compose up -d && docker compose ps -a && docker ps && docker volume ls && docker network ls
```

Here's how to build the image for the API (this needs to be ran from the sln dir because Docker
appears to not like parent directory traversals):

```shell
# docker build -t id-card-api:$(date +%s) -f ./src/Api/Dockerfile .
docker build -t id-card-api:latest -f ./src/Api/Dockerfile .
```

Here's an all-in-one command to create the image and run it (attached).

```shell
docker build -t id-card-api:latest -f ./src/Api/Dockerfile . \
  && docker run -p "[::1]:8080:8080" -e "ASPNETCORE_ENVIRONMENT=Development" id-card-api:latest
```

Here's a helpful lil command to clean up many images within a tag (I think this can be improved with `-q`):

```shell
docker image rm $(docker image ls | grep "^id-card-api" | awk -F' ' '{print $1 ":" $2}')
```

Start the service `docker compose up --build idcardapi_api -d`

Don't forget you can easily run and attach to an image with

```shell
docker run -it mcr.microsoft.com/dotnet/aspnet:8.0 /bin/bash
```

If the image has an entrypoint, this works too:

```shell
docker run -it --entrypoint /bin/bash docker-compose-lab-idcardapi-idcardapi_api
```

## Debugging within Docker

### Rootless

[Great Rider article](https://blog.jetbrains.com/dotnet/2023/08/16/debugging-docker-and-docker-compose-solutions-with-jetbrains-rider/)

To debug in Rider on Linux, either you need to run Rider as admin or setup Docker to run as rootless.

WARNING: this doesn't have access to .NET user secrets and it won't read from `launchSettings.json` and it won't load
`compose.override.yml`, so it's kind of a moot point, but here are the instructions anyways.

1. Install rootless docker
2. If `docker compose up -d` fails during pulling due to `docker-credential-desktop` being missing,
   `vi ~/.docker/config.json` and change `credsStore` to `credStore`
3. Configure Rider's Docker settings to use the rootless docker socket
4. If Rider doesn't have a run config for the API's compose service, create that.
5. Make sure the run config is configured appropriately
   1. `docker compose up` should `Attach to: None`. This will cause Rider to debug the launched API
   2. `docker compose up` should `Start: Selected services`. Normally, Rider will start/restart all
   dependencies. However, Rider doesn't respect `compose.override.yml`, so DB access tools like
   DataGrips won't be able to access the DBs (but this may not be the end of the world). This may
   also cause weirdness when debugging multiple apps, so managing the dependencies outside of Rider
   seems superficially superior.
6. Run the API's compose service in Debug mode
    - WARNING: Rider doesn't apply `compose.override.yml`!! It will at least make sure the API's
      port is exposed, but the other services won't be available on the host machine, and since
      Rider runs on the host, the DBs won't be accessible. As such, it can be helpful to here to
      make a service that contains the dependencies for the API, start that service normally, and
      then have Rider start the API itself
