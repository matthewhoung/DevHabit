namespace DevHabit.Api.Entities;

public sealed class EntryImportJob
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public EntryImportStatus Status { get; set; }
    public string FileName { get; set; }
    public byte[] FileContent { get; set; }
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public static string NewId() => $"ei_{Guid.CreateVersion7()}";
}
