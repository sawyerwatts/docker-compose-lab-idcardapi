# Docker Compose Lab: ID Card API

This repository is a helper to [docker-compose-lab](https://github.com/sawyerwatts/docker-compose-lab). See that repo
for more details, but the TLDR is that this repository contains a containerized .NET API.

For all the commands in this file, the following alias exists (because Docker on Linux):

```shell
# Normally:
sudo systemctl start docker

# If rootless:
systemctl --user start docker
```

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

*NOTE*: A lot of these all-in-one commands can be made simpler with `docker compose SVC -q`

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

## Debugging

### Rootless

[Great Rider article](https://blog.jetbrains.com/dotnet/2023/08/16/debugging-docker-and-docker-compose-solutions-with-jetbrains-rider/)

To debug in Rider on Linux, either you need to run Rider as admin or setup Docker to run as rootless.

1. Install rootless docker
2. If `docker compose up -d` fails during pulling due to `docker-credential-desktop` being missing,
   `vi ~/.docker/config.json` and change `credsStore` to `credStore`
3. Configure Rider's Docker settings to use the rootless docker socket
4. If Rider doesn't have a run config for the API's compose service, create that
5. Run the API's compose service in Debug mode
    - WARNING: Rider doesn't apply `compose.override.yml`!! It will at least make sure the API's
      port is exposed, but the other services won't be available on the host machine, and since
      Rider runs on the host, the DBs won't be accessible. As such, it can be helpful to here to
      make a service that contains the dependencies for the API, start that service normally, and
      then have Rider start the API itself

### TODOs

- is there a way to configure the port rider exports the api to? and to export when running w/o
  debugging?
- try out making a dependency service to start normally
- how speed up startup, and/or to autobuild the code on changes?
    - [have a dotnet run watch branch in the dockerfile?](https://learn.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch?view=aspnetcore-9.0#run-net-cli-commands-using-dotnet-watch)
    - make a `.dockerignore` for .NET build artifacts (go steal Rider's)
- take a pass at existing READMEs
    - put env var overrides into `launchSettings.json` so can code/debug normally, or when need to
      profile from startup
