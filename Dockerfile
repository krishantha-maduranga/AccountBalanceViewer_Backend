# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AccountBalanceViewer.Api/AccountBalanceViewer.Api.csproj", "AccountBalanceViewer.Api/"]
COPY ["AccountBalanceViewer.Application/AccountBalanceViewer.Application.csproj", "AccountBalanceViewer.Application/"]
COPY ["AccountBalanceViewer.Domain/AccountBalanceViewer.Domain.csproj", "AccountBalanceViewer.Domain/"]
COPY ["AccountBalanceViewer.Persistence/AccountBalanceViewer.Persistence.csproj", "AccountBalanceViewer.Persistence/"]
RUN dotnet restore "AccountBalanceViewer.Api/AccountBalanceViewer.Api.csproj"
COPY . .

WORKDIR /src/AccountBalanceViewer.Api
RUN dotnet build "./AccountBalanceViewer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AccountBalanceViewer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountBalanceViewer.Api.dll"]