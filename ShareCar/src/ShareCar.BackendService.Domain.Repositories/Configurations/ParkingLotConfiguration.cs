using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories.Configurations;

internal sealed class ParkingLotConfiguration : IEntityTypeConfiguration<ParkingLot>
{
  public void Configure(EntityTypeBuilder<ParkingLot> builder)
  {
    builder.ToTable("ParkingLots");
    builder.HasKey(p => p.Id);
    builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
    builder.Property(p => p.Latitude).IsRequired();
    builder.Property(p => p.Longitude).IsRequired();
    builder.Property(p => p.TotalCapacity).IsRequired();
  }
}
