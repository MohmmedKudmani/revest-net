using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Configuration;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Exceptions;
using OrderService.GrpcClients;
using OrderService.Models;
using Revest.Contracts;

namespace OrderService.Services;

public class OrdersService
{
    private readonly AppDbContext _db;
    private readonly OrderOptions _options;
    private readonly ProductGrpcClient _grpcClient;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IValidator<UpdateOrderDto> _updateValidator;

    public OrdersService(
        AppDbContext db,
        IOptions<OrderOptions> options,
        ProductGrpcClient grpcClient,
        IValidator<CreateOrderDto> createValidator,
        IValidator<UpdateOrderDto> updateValidator)
    {
        _db = db;
        _options = options.Value;
        _grpcClient = grpcClient;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        await _createValidator.ValidateAndThrowAsync(dto, ct);

        var product = await _grpcClient.GetProductAsync(dto.ProductId, ct);
        if (product is null)
        {
            throw new NotFoundException("Product not found");
        }

        // Fast-fail before the gRPC decrement round-trip
        if (product.Stock < dto.Quantity)
        {
            throw new BadRequestException(
                $"Insufficient stock: requested {dto.Quantity}, available {product.Stock}");
        }

        await _grpcClient.DecrementStockAsync(dto.ProductId, dto.Quantity, ct);

        var order = new Order
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            TotalPrice = Math.Round((decimal)product.Price * dto.Quantity, 2),
            Status = "PENDING",
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return ToDto(order, ToProductDetail(product));
    }

    public async Task<PaginatedResult<OrderResponseDto>> GetAllAsync(
        OrderQueryDto query,
        CancellationToken ct = default)
    {
        List<string>? productIds = null;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var matchingProducts = await _grpcClient.SearchProductsAsync(query.Search, ct);
            if (matchingProducts is null || !matchingProducts.Any())
            {
                return EmptyPaginatedResult(query);
            }

            productIds = matchingProducts.Select(p => p.Id).ToList();
        }

        var q = _db.Orders.AsNoTracking();

        if (productIds is not null)
        {
            q = q.Where(o => productIds.Contains(o.ProductId));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            q = q.Where(o => o.Status == query.Status);
        }

        q = (query.SortBy?.ToLower(), query.SortOrder?.ToLower()) switch
        {
            ("status", "asc") => q.OrderBy(o => o.Status),
            ("status", _) => q.OrderByDescending(o => o.Status),
            ("totalprice", "asc") => q.OrderBy(o => (double)o.TotalPrice),
            ("totalprice", _) => q.OrderByDescending(o => (double)o.TotalPrice),
            ("productid", "asc") => q.OrderBy(o => o.ProductId),
            ("productid", _) => q.OrderByDescending(o => o.ProductId),
            ("createdat", "asc") => q.OrderBy(o => o.CreatedAt),
            _ => q.OrderByDescending(o => o.CreatedAt),
        };

        var total = await q.CountAsync(ct);

        var limit = Math.Min(
            query.Limit > 0 ? query.Limit : _options.DefaultPageSize,
            _options.MaxPageSize);
        var page = query.Page > 0 ? query.Page : 1;

        var orders = await q
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(ct);

        var items = new List<OrderResponseDto>(orders.Count);
        foreach (var order in orders)
        {
            var product = await _grpcClient.GetProductAsync(order.ProductId, ct);
            items.Add(ToDto(order, product is not null ? ToProductDetail(product) : null));
        }

        var totalPages = (int)Math.Ceiling((double)total / limit);

        return new PaginatedResult<OrderResponseDto>(
            Data: items,
            Page: page,
            Limit: limit,
            Total: total,
            TotalPages: totalPages,
            HasNextPage: page < totalPages,
            HasPrevPage: page > 1
        );
    }

    public async Task<OrderResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        var product = await _grpcClient.GetProductAsync(order.ProductId, ct);
        return ToDto(order, product is not null ? ToProductDetail(product) : null);
    }

    public async Task<OrderResponseDto> UpdateAsync(
        Guid id,
        UpdateOrderDto dto,
        CancellationToken ct = default)
    {
        await _updateValidator.ValidateAndThrowAsync(dto, ct);

        var order = await _db.Orders.FindAsync([id], ct);
        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        if (dto.Status == "CANCELLED" && order.Status != "CANCELLED")
        {
            await _grpcClient.RestoreStockAsync(order.ProductId, order.Quantity, ct);
        }

        order.Status = dto.Status;
        await _db.SaveChangesAsync(ct);

        var product = await _grpcClient.GetProductAsync(order.ProductId, ct);
        return ToDto(order, product is not null ? ToProductDetail(product) : null);
    }

    public async Task<OrderResponseDto> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync([id], ct);
        if (order is null)
        {
            throw new NotFoundException("Order not found");
        }

        // Guard: only restore stock if the order wasn't already cancelled
        if (order.Status != "CANCELLED")
        {
            await _grpcClient.RestoreStockAsync(order.ProductId, order.Quantity, ct);
        }

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);

        return ToDto(order, null);
    }

    private static ProductDetailDto ToProductDetail(ProductMessage p) =>
        new(p.Id, p.Name, (decimal)p.Price, p.Stock);

    private static OrderResponseDto ToDto(Order o, ProductDetailDto? product) =>
        new(o.Id, o.ProductId, o.Quantity, o.TotalPrice, o.Status, o.CreatedAt, o.UpdatedAt, product);

    private static PaginatedResult<OrderResponseDto> EmptyPaginatedResult(OrderQueryDto query)
    {
        var limit = query.Limit > 0 ? query.Limit : 10;
        var page = query.Page > 0 ? query.Page : 1;
        return new PaginatedResult<OrderResponseDto>(
            Data: [],
            Page: page,
            Limit: limit,
            Total: 0,
            TotalPages: 0,
            HasNextPage: false,
            HasPrevPage: false
        );
    }
}
