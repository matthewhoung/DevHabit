using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.DTOs.HabitTags;
using DevHabit.Api.DTOs.Tags;
using DevHabit.Api.DTOs.Entries;
using DevHabit.Api.Entities;

namespace DevHabit.IntegrationTests.Infrastructure;

public static class TestData
{
    public static class Habits
    {
        public static CreateHabitDto CreateReadingHabit() => new()
        {
            Name = "Read Books",
            Type = HabitType.Measurable,
            Frequency = new FrequencyDto
            {
                Type = FrequencyType.Daily,
                TimesPerPeriod = 1
            },
            Target = new TargetDto
            {
                Value = 30,
                Unit = "pages"
            }
        };

        public static CreateHabitDto CreateExerciseHabit() => new()
        {
            Name = "Exercise",
            Type = HabitType.Measurable,
            Frequency = new FrequencyDto
            {
                Type = FrequencyType.Daily,
                TimesPerPeriod = 1
            },
            Target = new TargetDto
            {
                Value = 30,
                Unit = "minutes"
            }
        };
    }

    public static class Tags
    {
        public static CreateTagDto CreateImportantTag() => new()
        {
            Name = "Important"
        };

        public static CreateTagDto CreateProductivityTag() => new()
        {
            Name = "Productivity"
        };
    }

    public static class HabitTags
    {
        public static UpsertHabitTagsDto CreateUpsertDto(params string[] tagIds) => new()
        {
            TagIds = tagIds.ToList()
        };
    }

    public static class Entries
    {
        public static CreateEntryDto CreateEntry(string habitId, int value = 10, string? note = null) => new()
        {
            HabitId = habitId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Value = value,
            Notes = note
        };

        public static CreateEntryDto CreateEntryForDate(string habitId, DateOnly date, int value = 10, string? note = null) => new()
        {
            HabitId = habitId,
            Date = date,
            Value = value,
            Notes = note
        };

        public static UpdateEntryDto CreateUpdateEntry(int value = 20, string? note = "Updated entry") => new()
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Value = value,
            Notes = note
        };

        public static CreateEntryBatchDto CreateBatch(string habitId, params (DateOnly Date, int Value)[] entries) =>
            new()
            {
                Entries = entries.Select(e => new CreateEntryDto()
                {
                    HabitId = habitId,
                    Date = e.Date, 
                    Value = e.Value 
                }).ToList()
            };
    }
} 
