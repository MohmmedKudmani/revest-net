namespace OrderService.DTOs;

public record PaginatedResult<T>(
    IEnumerable<T> Data,
    int Page,
    int Limit,
    int Total,
    int TotalPages,
    bool HasNextPage,
    bool HasPrevPage
);
