namespace OrderService.DTOs;

public record OrderResponseDto(
    Guid Id,
    string ProductId,
    int Quantity,
    decimal TotalPrice,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    ProductDetailDto? Product
);
