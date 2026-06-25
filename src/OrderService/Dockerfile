FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Root-only setup: health-check tool + writable data dir owned by the non-root app user
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/data \
    && chown -R app:app /app/data
EXPOSE 3002
USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/OrderService/OrderService.csproj src/OrderService/
COPY src/Revest.Contracts/Revest.Contracts.csproj src/Revest.Contracts/
RUN dotnet restore src/OrderService/OrderService.csproj
COPY src/OrderService/ src/OrderService/
COPY src/Revest.Contracts/ src/Revest.Contracts/
RUN dotnet publish src/OrderService/OrderService.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
