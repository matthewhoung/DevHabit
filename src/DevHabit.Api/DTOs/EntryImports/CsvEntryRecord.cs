using CsvHelper.Configuration.Attributes;

namespace DevHabit.Api.DTOs.EntryImports;

public sealed class CsvEntryRecord
{
    [Name("habit_id")]
    public required string HabitId { get; init; }

    [Name("date")]
    public required DateOnly Date { get; init; }

    [Name("notes")]
    public string? Notes { get; init; }
}
