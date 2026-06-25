namespace ProductService.DTOs;

public record ProductQueryDto(
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStock = null,
    string? SortBy = null,
    string? SortOrder = null,
    int Page = 1,
    int Limit = 10
);
