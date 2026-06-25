# Revest .NET — Project Context for Claude

## What This Is

A .NET 8 rebuild of a product + order management system. Three runnable projects:

| Project | Type | Port |
|---|---|---|
| `ProductService` | ASP.NET Core Web API | HTTP :3001 · gRPC :5100 |
| `OrderService` | ASP.NET Core Web API | HTTP :3002 |
| `RevestAdmin` | Blazor Server | HTTP :5000 |
| `Revest.Contracts` | Class library | gRPC proto only, not runnable |

## Running the Projects

```bash
# Each in a separate terminal
dotnet run --project src/ProductService
dotnet run --project src/OrderService
dotnet run --project src/RevestAdmin

# Build everything
dotnet build

# Format all code
dotnet format
```

## Plans

Detailed implementation plans are in `plan/` (git-ignored — local only):
- `plan/README.md` — architecture + contract invariants
- `plan/01-product-service.md`
- `plan/02-order-service.md`
- `plan/03-frontend.md`

## API Contract Rules — Never Deviate From These

### Error shape (all non-2xx):
```json
{ "status": 404, "message": "Not found" }
```
Only two fields. `message` can be `string` or `string[]` (validation errors).

### Response wrapper — writes only:
- `POST / PATCH / DELETE` → `{ "message": "Product \"X\" created successfully", "data": { ... } }`
- `GET` → raw object or paginated result, no wrapper

### Pagination shape:
```json
{ "data": [], "page": 1, "limit": 10, "total": 0, "totalPages": 0, "hasNextPage": false, "hasPrevPage": false }
```

### Stock decrement — must be atomic:
Use `ExecuteUpdateAsync` with `.Where(p => p.Id == id && p.Stock >= qty)`.
Check affected row count. Never read-then-write.

## Coding Rules

- All DTOs are `record` types
- All reads use `AsNoTracking()`
- All async methods accept `CancellationToken ct` and pass it through
- Use `IOptions<T>` for config — never `Environment.GetEnvironmentVariable` directly
- Use `ServiceCollectionExtensions.AddApplicationServices(...)` to keep `Program.cs` clean
- Never expose EF entity classes from controllers — always map to DTOs
- Do NOT use the Result pattern — use exceptions + global exception filter
- camelCase JSON serialization policy is global (C# `TotalPages` → `totalPages` in JSON)

## gRPC

The `.proto` file lives in `src/Revest.Contracts/Protos/product.proto`.
- ProductService: implements the gRPC server (port 5100, HTTP/2)
- OrderService: uses the gRPC client (calls ProductService on port 5100)
- RevestAdmin: never uses gRPC — REST only

## Skills Available

- `/dotnet-grpc` — gRPC server/client setup, proto definition, Kestrel config
- `/dotnet-backend-patterns` — EF Core, DI, async, validation patterns

## Formatting

`dotnet format` runs automatically after every file edit (Claude hook).
`.editorconfig` defines the rules — 4-space indent for C#, file-scoped namespaces.
