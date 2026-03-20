# ── Build stage ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solution y csproj primero (aprovecha cache de capas)
COPY src/RouteOps.Domain/RouteOps.Domain.csproj         src/RouteOps.Domain/
COPY src/RouteOps.Application/RouteOps.Application.csproj src/RouteOps.Application/
COPY src/RouteOps.Infrastructure/RouteOps.Infrastructure.csproj src/RouteOps.Infrastructure/
COPY src/RouteOps.API/RouteOps.API.csproj               src/RouteOps.API/

# Restaurar dependencias
RUN dotnet restore src/RouteOps.API/RouteOps.API.csproj

# Copiar todo el código fuente
COPY src/ src/

# Publicar en modo Release
RUN dotnet publish src/RouteOps.API/RouteOps.API.csproj \
    -c Release \
    -o /app/out \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

# Railway inyecta $PORT automáticamente
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RouteOps.API.dll"]