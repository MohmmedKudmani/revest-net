using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OrderService.Extensions;
using OrderService.Filters;
using OrderService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Controllers with camelCase JSON + global exception filter
builder.Services.AddControllers(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// Application services: EF Core, OrdersService, OrderOptions, validators, gRPC client
builder.Services.AddApplicationServices(builder.Configuration);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kestrel: HTTP/1.1 REST on port 3002 only — no gRPC server
builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenAnyIP(3002, o => o.Protocols = HttpProtocols.Http1);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderService.Data.AppDbContext>();
    db.Database.EnsureCreated();
}

// Swagger UI available in all environments (learning project)
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

// Wrap POST/PATCH/DELETE success responses in { message, data }
app.UseMiddleware<ResponseWrapperMiddleware>();

app.MapControllers();

app.Run();
