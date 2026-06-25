using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductService.Configuration;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Services;
using ProductService.Validators;

namespace ProductService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Data Source=products.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ProductsService>();

        services.Configure<CatalogOptions>(
            configuration.GetSection(CatalogOptions.SectionName));

        services.AddScoped<IValidator<CreateProductDto>, CreateProductValidator>();
        services.AddScoped<IValidator<UpdateProductDto>, UpdateProductValidator>();

        return services;
    }
}
