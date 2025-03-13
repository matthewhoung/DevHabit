using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(500);
        builder.Property(e => e.HabitId).HasMaxLength(500);
        builder.Property(e => e.UserId).HasMaxLength(500);

        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Property(e => e.ExternalId).HasMaxLength(1000);

        builder.HasOne(e => e.Habit)
            .WithMany()
            .HasForeignKey(e => e.HabitId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

        // We have to match snake_case naming convention for the column name
        builder.HasIndex(e => e.ExternalId)
            .IsUnique()
            .HasFilter("external_id IS NOT NULL");
    }
}
