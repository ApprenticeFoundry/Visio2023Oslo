#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

ARG NugetUserId
ARG NugetPersonalAccessToken

ENV NugetUserId=${NugetUserId}
ENV NugetPersonalAccessToken=${NugetPersonalAccessToken}

WORKDIR /src
COPY ["OSLOFoundry.csproj", "nuget.config", "./"]

RUN dotnet restore "./OSLOFoundry.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "OSLOFoundry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OSLOFoundry.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/storage
COPY ./Storage/ ./storage/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OSLOFoundry.dll"]

# az login
# az acr login --name iobtassets
# From OSLOFoundry folder:
# Windows Environment Variables
# set NugetUserId=iobt
# set NugetPersonalAccessToken=<personal access token from https://dev.azure.com/iobt/IoBTNuGet/_artifacts/feed/BlazorThreeJS>
# docker build -t oslofoundry --build-arg NugetUserId --build-arg NugetPersonalAccessToken .
# docker tag OSLOFoundry iobtassets.azurecr.io/OSLOFoundry:v2.0.0
# docker push iobtassets.azurecr.io/OSLOFoundry:v2.0.0
# Note: cannot be run from localhost. Use machine IP.  For example:  http://192.168.1.165:5200/
# docker run -d -p 5200:80 --rm --name OSLOFoundry OSLOFoundry
# docker run -it  OSLOFoundry /bin/bash
# docker exec -it OSLOFoundry bash