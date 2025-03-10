﻿using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.DTOs.Common;

public sealed record PaginationResult<T> : ICollectionResponse<T>, ILinksResponse
{
    public List<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public List<LinkDto> Links { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static async Task<PaginationResult<T>> CreateAsync(
        IQueryable<T> query,
        int page,
        int pageSize)
    {
        int totalCount = await query.CountAsync();
        List<T> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PaginationResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
