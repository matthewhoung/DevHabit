using DevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.Api.DTOs.Habits;

public sealed record class HabitsQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public HabitType? Type { get; init; }
    public HabitStatus? Status { get; init; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }
}
