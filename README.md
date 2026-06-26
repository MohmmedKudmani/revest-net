# Revest .NET

A product and order management system built with .NET 8. Two REST/gRPC microservices and a Blazor Server admin dashboard.

---

## About

Revest Admin lets you manage a product catalogue and track orders through a web dashboard. The system is split into three independently runnable services:

- **ProductService** — manages products (CRUD, stock tracking, search, pagination)
- **OrderService** — manages orders, talks to ProductService via gRPC to check and decrement stock
- **RevestAdmin** — Blazor Server web UI for managing both products and orders

Services communicate internally via gRPC. The admin UI talks to both services over REST.

---

## Project Structure

```
revest-net/
├── src/
│   ├── ProductService/         # ASP.NET Core Web API — products
│   ├── OrderService/           # ASP.NET Core Web API — orders
│   ├── RevestAdmin/            # Blazor Server — admin dashboard
│   └── Revest.Contracts/       # Shared class library — gRPC proto definition
├── docker-compose.yml
├── revest-net.sln
├── .editorconfig               # Code formatting rules
└── .dockerignore
```

### Ports

| Service | Protocol | Port |
|---|---|---|
| ProductService | HTTP (REST + Swagger) | 3001 |
| ProductService | HTTP/2 (gRPC) | 5100 |
| OrderService | HTTP (REST + Swagger) | 3002 |
| RevestAdmin | HTTP (Blazor) | 5000 |

### Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 / .NET 8 |
| APIs | ASP.NET Core Web API (Controllers) |
| Frontend | Blazor Server + MudBlazor |
| Database | SQLite via Entity Framework Core |
| Inter-service | gRPC (Revest.Contracts shared proto) |
| Validation | FluentValidation |
| API docs | Swagger (Swashbuckle) |
| Containers | Docker + Docker Compose |

---

## Development Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [dotnet-ef](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) global tool

```bash
dotnet tool install --global dotnet-ef
```

### Run

Each service runs in its own terminal with hot reload:

```bash
# Terminal 1
dotnet watch --project src/ProductService

# Terminal 2
dotnet watch --project src/OrderService

# Terminal 3
dotnet watch --project src/RevestAdmin
```

### Seed Products (run once)

```bash
dotnet run --project src/ProductService -- --seed
```

### Other Commands

```bash
# Build entire solution
dotnet build

# Format all code
dotnet format
```

### Dev URLs

| URL | Description |
|---|---|
| `http://localhost:5000` | Admin dashboard |
| `http://localhost:3001/swagger` | ProductService API docs |
| `http://localhost:3002/swagger` | OrderService API docs |

---

## Production Setup (Docker)

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine + Compose v2

### Start Everything

```bash
docker compose up -d --build
```

### Seed Products (run once after first start)

```bash
docker compose run --rm product-service --seed
```

### View Running Containers

```bash
docker compose ps
```

### View Logs

```bash
# All services
docker compose logs -f

# Single service
docker compose logs -f product-service
```

### Stop

```bash
docker compose down
```

### Stop and Wipe Databases

```bash
docker compose down -v
```

> This deletes the SQLite data volumes. You will need to re-seed after running this.

### Rebuild a Single Service

```bash
docker compose up -d --build product-service
```

### Prod URLs

| URL | Description |
|---|---|
| `http://localhost:5000` | Admin dashboard |
| `http://localhost:3001/swagger` | ProductService API docs |
| `http://localhost:3002/swagger` | OrderService API docs |

---

## API Overview

### Error shape (all non-2xx responses)

```json
{ "status": 404, "message": "Not found" }
```

### Write response wrapper (POST / PATCH / DELETE)

```json
{ "message": "Product \"X\" created successfully", "data": { ... } }
```

### GET responses

Raw object or paginated result — no wrapper.

```json
{
  "data": [],
  "page": 1,
  "limit": 10,
  "total": 0,
  "totalPages": 0,
  "hasNextPage": false,
  "hasPrevPage": false
}
```
