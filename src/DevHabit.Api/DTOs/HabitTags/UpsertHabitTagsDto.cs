namespace DevHabit.Api.DTOs.HabitTags;

public sealed record class UpsertHabitTagsDto
{
    public required List<string> TagIds { get; init; }
}
