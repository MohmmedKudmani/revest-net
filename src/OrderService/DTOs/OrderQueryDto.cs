namespace OrderService.DTOs;

public record OrderQueryDto(
    string? Search = null,
    string? Status = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int Limit = 10
);
