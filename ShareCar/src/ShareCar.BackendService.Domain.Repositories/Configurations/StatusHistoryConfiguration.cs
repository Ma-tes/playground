using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories.Configurations;

internal sealed class StatusHistoryConfiguration : IEntityTypeConfiguration<StatusHistory>
{
  public void Configure(EntityTypeBuilder<StatusHistory> builder)
  {
    builder.ToTable("StatusHistories");
    builder.HasKey(s => s.Id);
    builder.Property(s => s.OldStatus).IsRequired();
    builder.Property(s => s.NewStatus).IsRequired();
    builder.Property(s => s.ChangedAt).IsRequired();
    builder.Property(s => s.TriggeredBy).HasMaxLength(200).IsRequired();
    builder.HasIndex(s => s.VehicleId);
    builder.HasOne<Vehicle>().WithMany().HasForeignKey(s => s.VehicleId);
  }
}
