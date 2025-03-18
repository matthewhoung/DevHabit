using DevHabit.Api.Database;
using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace DevHabit.Api.Jobs;

public sealed class CleanupEntryImportJobsJob(
    ApplicationDbContext dbContext,
    ILogger<CleanupEntryImportJobsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            // Delete completed jobs older than 7 days
            DateTime completedJobsCutoffDate = DateTime.UtcNow.AddDays(-7);

            int deletedCount = await dbContext.EntryImportJobs
                .Where(j => j.Status == EntryImportStatus.Completed)
                .Where(j => j.CompletedAtUtc < completedJobsCutoffDate)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
            {
                logger.LogInformation("Deleted {Count} old import jobs", deletedCount);
            }

            // Delete failed jobs older than 30 days
            DateTime failedJobsCutoffDate = DateTime.UtcNow.AddDays(-30);

            deletedCount = await dbContext.EntryImportJobs
                .Where(j => j.Status == EntryImportStatus.Failed)
                .Where(j => j.CompletedAtUtc < failedJobsCutoffDate)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
            {
                logger.LogInformation("Deleted {Count} old failed import jobs", deletedCount);
            }

            // Delete stuck jobs (processing for more than 2 hour)
            DateTime processingJobsCutoffDate = DateTime.UtcNow.AddHours(-2);

            deletedCount = await dbContext.EntryImportJobs
                .Where(j => j.Status == EntryImportStatus.Processing)
                .Where(j => j.CreatedAtUtc < processingJobsCutoffDate)
                .ExecuteDeleteAsync();

            if (deletedCount > 0)
            {
                logger.LogWarning("Deleted {Count} stuck import jobs", deletedCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cleaning up old import jobs");
        }
    }
}
