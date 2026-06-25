namespace ProductService.DTOs;

public record CreateProductDto(
    string Name,
    string? Description,
    decimal Price,
    int Stock = 0
);
