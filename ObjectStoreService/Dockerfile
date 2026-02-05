# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
# Internal port
EXPOSE 8000


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ObjectStoreService.API/ObjectStoreService.API.csproj", "ObjectStoreService.API/"]
COPY ["ObjectStoreService.Application/ObjectStoreService.Application.csproj", "ObjectStoreService.Application/"]
COPY ["ObjectStoreService.Domain/ObjectStoreService.Domain.csproj", "ObjectStoreService.Domain/"]
COPY ["ObjectStoreService.Infrastructure/ObjectStoreService.Infrastructure.csproj", "ObjectStoreService.Infrastructure/"]
COPY ["ObjectStoreService.UnitTests/ObjectStoreService.UnitTests.csproj", "ObjectStoreService.UnitTests/"]
RUN dotnet restore "./ObjectStoreService.API/ObjectStoreService.API.csproj"
COPY . .
WORKDIR "/src/ObjectStoreService.API"
RUN dotnet build "./ObjectStoreService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS test

WORKDIR /src/ObjectStoreService.UnitTests

# If tests fail → docker build fails automatically
RUN dotnet test "ObjectStoreService.UnitTests.csproj" -c $BUILD_CONFIGURATION --verbosity normal

# This stage is used to publish the service project to be copied to the final stage
FROM test AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/ObjectStoreService.API
RUN dotnet publish "./ObjectStoreService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
RUN mkdir /app/wwwroot
RUN chmod -R 777 /app/wwwroot
ENTRYPOINT ["dotnet", "ObjectStoreService.API.dll"]
