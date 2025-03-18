using System.Linq.Expressions;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.EntryImports;

internal static class EntryImportQueries
{
    public static Expression<Func<EntryImportJob, EntryImportJobDto>> ProjectToDto()
    {
        return job => new EntryImportJobDto
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
