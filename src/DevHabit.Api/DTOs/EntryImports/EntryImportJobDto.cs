using DevHabit.Api.DTOs.Common;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.EntryImports;

public sealed record EntryImportJobDto
{
    public required string Id { get; init; }
    public required EntryImportStatus Status { get; init; }
    public required string FileName { get; init; }
    public required int TotalRecords { get; init; }
    public required int ProcessedRecords { get; init; }
    public required int SuccessfulRecords { get; init; }
    public required int FailedRecords { get; init; }
    public required List<string> Errors { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public List<LinkDto>? Links { get; set; }
}
