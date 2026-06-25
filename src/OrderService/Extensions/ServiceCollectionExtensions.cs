using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Configuration;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.GrpcClients;
using OrderService.Services;
using OrderService.Validators;
using Revest.Contracts;

namespace OrderService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Data Source=orders.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<OrdersService>();

        services.Configure<OrderOptions>(
            configuration.GetSection(OrderOptions.SectionName));

        services.AddScoped<IValidator<CreateOrderDto>, CreateOrderValidator>();
        services.AddScoped<IValidator<UpdateOrderDto>, UpdateOrderValidator>();

        services.AddScoped<ProductGrpcClient>();

        services.AddGrpcClient<ProductGrpcService.ProductGrpcServiceClient>(options =>
        {
            options.Address = new Uri(
                configuration["PRODUCT_SERVICE_GRPC_URL"] ?? "http://localhost:5100");
        });

        return services;
    }
}
