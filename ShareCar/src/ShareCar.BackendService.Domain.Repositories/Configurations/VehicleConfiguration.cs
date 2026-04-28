using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
  public void Configure(EntityTypeBuilder<Vehicle> builder)
  {
    builder.ToTable("Vehicles");
    builder.HasKey(v => v.Id);
    builder.Property(v => v.Model).HasMaxLength(200).IsRequired();
    builder.Property(v => v.PlateNumber).HasMaxLength(20).IsRequired();
    builder.HasIndex(v => v.PlateNumber).IsUnique();
    builder.Property(v => v.Status).IsRequired();
    builder.HasIndex(v => v.Status);
    builder.Property(v => v.Odometer).IsRequired().HasDefaultValue(0);
    builder.HasOne<ParkingLot>().WithMany().HasForeignKey(v => v.CurrentParkingLotId);
  }
}
