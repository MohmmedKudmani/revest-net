namespace RevestAdmin.Models;

public record Order(
    string Id,
    string ProductId,
    int Quantity,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ProductDetail? Product   // null if product was deleted or service unreachable
);

public record ProductDetail(string Id, string Name, decimal Price, int Stock);
public record CreateOrderRequest(string ProductId, int Quantity);
public record UpdateOrderStatusRequest(string Status);
public record OrderQuery(string? Search = null, string? Status = null, string? SortBy = null,
    string? SortOrder = null, int Page = 1, int Limit = 10);
