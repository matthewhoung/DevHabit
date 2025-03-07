namespace DevHabit.Api.Services.Sorting;

public sealed record class SortMapping(
    string SortField,
    string PropertyName,
    bool Reverse = false);
