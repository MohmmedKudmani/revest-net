using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProductService.Data;
using ProductService.Extensions;
using ProductService.Filters;
using ProductService.GrpcServices;
using ProductService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Controllers with camelCase JSON + global exception filter
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// Application services: EF Core, ProductsService, CatalogOptions, validators
builder.Services.AddApplicationServices(builder.Configuration);

// gRPC
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddGrpcReflection();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kestrel: HTTP/1.1 REST on 3001, HTTP/2 gRPC on 5100
builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenAnyIP(3001, o => o.Protocols = HttpProtocols.Http1);
    k.ListenAnyIP(5100, o => o.Protocols = HttpProtocols.Http2);
});

var app = builder.Build();

// Run migrations and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await Seeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Wrap POST/PATCH/DELETE success responses in { message, data }
app.UseMiddleware<ResponseWrapperMiddleware>();

app.MapControllers();
app.MapGrpcService<ProductGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
