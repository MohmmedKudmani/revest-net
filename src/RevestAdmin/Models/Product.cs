namespace RevestAdmin.Models;

public record Product(
    string Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateProductRequest(string Name, string? Description, decimal Price, int Stock);
public record UpdateProductRequest(string? Name, string? Description, decimal? Price, int? Stock);
public record ProductQuery(string? Search = null, decimal? MinPrice = null, decimal? MaxPrice = null,
    bool? InStock = null, string? SortBy = null, string? SortOrder = null, int Page = 1, int Limit = 10);
