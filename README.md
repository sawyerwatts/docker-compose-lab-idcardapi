# Docker Compose Lab: ID Card API

This repository is a helper to [docker-compose-lab](https://github.com/sawyerwatts/docker-compose-lab). See that repo
for more details, but the TLDR is that this repository contains a containerized .NET API.

For all the commands in this file, the following alias exists (because Docker on Linux):

```shell
alias sdocker='sudo docker'
```

## Building the docker image

Recall that the following can be used to interactively play with an image.

```shell
sdocker run -it IMAGE
```

This is a (stubbed) API to build ID cards from the database, and cache the result.

```shell
sdocker compose down -v; sdocker compose up -d && sdocker compose ps -a && sdocker ps && sdocker volume ls && sdocker network ls
```

Here's how to build the image for the API (this needs to be ran from the sln dir because Docker
appears to not like parent directory traversals):

```shell
# sdocker build -t id-card-api:$(date +%s) -f ./src/Api/Dockerfile .
sdocker build -t id-card-api:latest -f ./src/Api/Dockerfile .
```

*NOTE*: A lot of these all-in-one commands can be made simpler with `docker compose SVC -q`

Here's an all-in-one command to create the image and run it (attached).

```shell
sdocker build -t id-card-api:latest -f ./src/Api/Dockerfile . \
  && sdocker run -p "[::1]:8080:8080" -e "ASPNETCORE_ENVIRONMENT=Development" id-card-api:latest
```

Here's a helpful lil command to clean up many images within a tag (I think this can be improved with `-q`):

```shell
sdocker image rm $(sdocker image ls | grep "^id-card-api" | awk -F' ' '{print $1 ":" $2}')
```

## Debugging

[Great Rider article](https://blog.jetbrains.com/dotnet/2023/08/16/debugging-docker-and-docker-compose-solutions-with-jetbrains-rider/)

To debug in Rider on Linux, either you need to run Rider as admin or setup Docker to run as rootless.

TODO: have a dependencies "service" to allow for easy starting of dependencies w/o the image itself?

TODO: have an easy way to access the api from host machine; export port?

TODO: make a `.dockerignore` for .NET build artifacts (go steal Rider's)

TODO: how speed up startup?

TODO: add apikey auth back in

TODO: take a pass at existing READMEs

1. Install rootless docker
2. If `docker compose up -d` fails during pulling due to `docker-credential-desktop` being missing,
   `vi ~/.docker/config.json` and change `credsStore` to `credStore`
3. Configure Rider's Docker settings to use the rootless docker socket
4. Start the service `docker compose up --build idcardapi_api -d`
4. TODO: this container, when ran by Rider, isn't on same network as docker compose, so can't find ports
   - how add everything to an external network?
5. TODO: now how debug?
