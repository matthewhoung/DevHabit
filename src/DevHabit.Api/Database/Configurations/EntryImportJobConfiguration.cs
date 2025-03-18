using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class EntryImportJobConfiguration : IEntityTypeConfiguration<EntryImportJob>
{
    public void Configure(EntityTypeBuilder<EntryImportJob> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(500);
        builder.Property(e => e.UserId).HasMaxLength(500);
        builder.Property(e => e.UserId).HasMaxLength(500);
        builder.Property(e => e.FileName).HasMaxLength(500);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId);
    }
}
