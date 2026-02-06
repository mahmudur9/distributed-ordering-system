# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
# Internal port
EXPOSE 8000
EXPOSE 5000


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PaymentService.API/PaymentService.API.csproj", "PaymentService.API/"]
COPY ["PaymentService.Application/PaymentService.Application.csproj", "PaymentService.Application/"]
COPY ["PaymentService.Domain/PaymentService.Domain.csproj", "PaymentService.Domain/"]
COPY ["PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "PaymentService.Infrastructure/"]
COPY ["PaymentService.UnitTests/PaymentService.UnitTests.csproj", "PaymentService.UnitTests/"]
RUN dotnet restore "./PaymentService.API/PaymentService.API.csproj"
COPY . .
WORKDIR "/src/PaymentService.API"
RUN dotnet build "./PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS test

WORKDIR /src/PaymentService.UnitTests

# If tests fail → docker build fails automatically
RUN dotnet test "PaymentService.UnitTests.csproj" -c $BUILD_CONFIGURATION --verbosity normal

# This stage is used to publish the service project to be copied to the final stage
FROM test AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/PaymentService.API
RUN dotnet publish "./PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
USER root
RUN mkdir -p /app/logs && chmod -R 777 /app/logs
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
