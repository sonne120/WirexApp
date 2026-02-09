# Read Service Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src


COPY ["WirexApp.ReadService/WirexApp.ReadService.csproj", "WirexApp.ReadService/"]
COPY ["WirexApp.Application/WirexApp.Application.csproj", "WirexApp.Application/"]
COPY ["WirexApp.Domain/WirexApp.Domain.csproj", "WirexApp.Domain/"]
COPY ["WirexApp.Infrastructure/WirexApp.Infrastructure.csproj", "WirexApp.Infrastructure/"]

RUN dotnet restore "WirexApp.ReadService/WirexApp.ReadService.csproj"

COPY . .

WORKDIR "/src/WirexApp.ReadService"
RUN dotnet build "WirexApp.ReadService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WirexApp.ReadService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WirexApp.ReadService.dll"]
