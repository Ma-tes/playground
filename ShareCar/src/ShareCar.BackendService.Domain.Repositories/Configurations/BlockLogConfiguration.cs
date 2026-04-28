using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories.Configurations;

internal sealed class BlockLogConfiguration : IEntityTypeConfiguration<BlockLog>
{
  public void Configure(EntityTypeBuilder<BlockLog> builder)
  {
    builder.ToTable("BlockLogs");
    builder.HasKey(b => b.Id);
    builder.Property(b => b.StartTime).IsRequired();
    builder.Property(b => b.Reason).HasMaxLength(500).IsRequired();
    builder.HasIndex(b => b.VehicleId);
    builder.HasOne<Vehicle>().WithMany().HasForeignKey(b => b.VehicleId);
    builder.HasOne<User>().WithMany().HasForeignKey(b => b.AdminId);
  }
}
