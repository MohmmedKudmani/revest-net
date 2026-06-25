using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductService.Configuration;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Exceptions;
using ProductService.Models;

namespace ProductService.Services;

public class ProductsService
{
    private readonly AppDbContext _db;
    private readonly CatalogOptions _options;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductsService(
        AppDbContext db,
        IOptions<CatalogOptions> options,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator)
    {
        _db = db;
        _options = options.Value;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PaginatedResult<ProductResponseDto>> GetAllAsync(
        ProductQueryDto query,
        CancellationToken ct = default)
    {
        var q = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            q = q.Where(p => EF.Functions.Like(p.Name, $"%{query.Search}%"));
        }

        if (query.MinPrice.HasValue)
        {
            q = q.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            q = q.Where(p => p.Price <= query.MaxPrice.Value);
        }

        if (query.InStock == true)
        {
            q = q.Where(p => p.Stock > 0);
        }

        q = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
        {
            ("price", "desc") => q.OrderByDescending(p => (double)p.Price),
            ("price", _) => q.OrderBy(p => (double)p.Price),
            ("stock", "desc") => q.OrderByDescending(p => p.Stock),
            ("stock", _) => q.OrderBy(p => p.Stock),
            ("createdat", "desc") => q.OrderByDescending(p => p.CreatedAt),
            ("createdat", _) => q.OrderBy(p => p.CreatedAt),
            ("name", "desc") => q.OrderByDescending(p => p.Name),
            _ => q.OrderBy(p => p.Name),
        };

        var total = await q.CountAsync(ct);

        var limit = Math.Min(
            query.Limit > 0 ? query.Limit : _options.DefaultPageSize,
            _options.MaxPageSize);
        var page = query.Page > 0 ? query.Page : 1;

        var items = await q
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)total / limit);

        return new PaginatedResult<ProductResponseDto>(
            Data: items,
            Page: page,
            Limit: limit,
            Total: total,
            TotalPages: totalPages,
            HasNextPage: page < totalPages,
            HasPrevPage: page > 1
        );
    }

    public async Task<ProductResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
        {
            throw new NotFoundException("Product not found");
        }

        return ToDto(product);
    }

    public async Task<ProductResponseDto> CreateAsync(
        CreateProductDto dto,
        CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductResponseDto> UpdateAsync(
        Guid id,
        UpdateProductDto dto,
        CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, ct);

        var product = await _db.Products.FindAsync([id], ct);
        if (product is null)
        {
            throw new NotFoundException("Product not found");
        }

        if (dto.Name is not null)
        {
            product.Name = dto.Name;
        }

        if (dto.Description is not null)
        {
            product.Description = dto.Description;
        }

        if (dto.Price is not null)
        {
            product.Price = dto.Price.Value;
        }

        if (dto.Stock is not null)
        {
            product.Stock = dto.Stock.Value;
        }

        await _db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductResponseDto> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct);
        if (product is null)
        {
            throw new NotFoundException("Product not found");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductResponseDto> DecrementStockAsync(
        Guid id,
        int qty,
        CancellationToken ct = default)
    {
        var affected = await _db.Products
            .Where(p => p.Id == id && p.Stock >= qty)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Stock, p => p.Stock - qty),
                ct);

        if (affected == 0)
        {
            var product = await _db.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (product is null)
            {
                throw new NotFoundException("Product not found");
            }

            throw new BadRequestException(
                $"Insufficient stock: requested {qty}, available {product.Stock}");
        }

        var updated = await _db.Products
            .AsNoTracking()
            .FirstAsync(p => p.Id == id, ct);

        return ToDto(updated);
    }

    public async Task<ProductResponseDto> RestoreStockAsync(
        Guid id,
        int qty,
        CancellationToken ct = default)
    {
        var affected = await _db.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Stock, p => p.Stock + qty),
                ct);

        if (affected == 0)
        {
            throw new NotFoundException("Product not found");
        }

        var updated = await _db.Products
            .AsNoTracking()
            .FirstAsync(p => p.Id == id, ct);

        return ToDto(updated);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllRawAsync(
        CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Select(p => ToDto(p))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ProductResponseDto>> SearchByNameAsync(
        string name,
        CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
            .Select(p => ToDto(p))
            .ToListAsync(ct);
    }

    private static ProductResponseDto ToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt, p.UpdatedAt);
}
