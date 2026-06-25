namespace ProductService.DTOs;

public record UpdateProductDto(
    string? Name,
    string? Description,
    decimal? Price,
    int? Stock
);
