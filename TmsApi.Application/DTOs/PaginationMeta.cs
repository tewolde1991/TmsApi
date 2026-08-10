namespace TmsApi.Application.DTOs;

public record PaginationMeta(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNext,
    bool HasPrevious
);