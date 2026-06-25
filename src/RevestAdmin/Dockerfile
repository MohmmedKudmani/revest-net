FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Root-only setup: health-check tool (no SQLite data dir needed for Blazor)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /home/app/.aspnet
EXPOSE 5000
USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/RevestAdmin/RevestAdmin.csproj src/RevestAdmin/
RUN dotnet restore src/RevestAdmin/RevestAdmin.csproj
COPY src/RevestAdmin/ src/RevestAdmin/
RUN dotnet publish src/RevestAdmin/RevestAdmin.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RevestAdmin.dll"]
