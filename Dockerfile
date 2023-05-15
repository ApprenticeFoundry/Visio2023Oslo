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
COPY ["Visio2023Foundry.csproj", "nuget.config", "./"]

RUN dotnet restore "./Visio2023Foundry.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Visio2023Foundry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Visio2023Foundry.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir -p /app/storage
COPY ./storage/ ./storage/
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Visio2023Foundry.dll"]

# az login
# az acr login --name iobtassets
# From Visio2023Foundry folder:
# Windows Environment Variables
# set NugetUserId=iobt
# set NugetPersonalAccessToken=<personal access token from https://dev.azure.com/iobt/IoBTNuGet/_artifacts/feed/BlazorThreeJS>
# docker build -t visio2023oslo --build-arg NugetUserId --build-arg NugetPersonalAccessToken .
# docker tag visio2023oslo iobtassets.azurecr.io/visio2023oslo:v1.0.0
# docker push iobtassets.azurecr.io/visio2023oslo:v1.0.0
# Note: cannot be run from localhost. Use machine IP.  For example:  http://192.168.1.165:5200/
# docker run -d -p 5200:80 --rm --name visio2023oslo visio2023oslo
# docker run -it  visio2023oslo /bin/bash
# docker exec -it visio2023oslo bash