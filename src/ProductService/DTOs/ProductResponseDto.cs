namespace ProductService.DTOs;

public record ProductResponseDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
