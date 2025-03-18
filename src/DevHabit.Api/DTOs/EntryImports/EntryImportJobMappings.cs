using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.EntryImports;

public static class EntryImportJobMappings
{
    public static EntryImportJobDto ToDto(this EntryImportJob job)
    {
        return new EntryImportJobDto
        {
            Id = job.Id,
            Status = job.Status,
            FileName = job.FileName,
            TotalRecords = job.TotalRecords,
            ProcessedRecords = job.ProcessedRecords,
            SuccessfulRecords = job.SuccessfulRecords,
            FailedRecords = job.FailedRecords,
            Errors = job.Errors,
            CreatedAtUtc = job.CreatedAtUtc,
            CompletedAtUtc = job.CompletedAtUtc
        };
    }
}
