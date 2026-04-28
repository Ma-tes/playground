using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
  public void Configure(EntityTypeBuilder<Booking> builder)
  {
    builder.ToTable("Bookings");
    builder.HasKey(b => b.Id);
    builder.Property(b => b.StartParkingLotId).IsRequired();
    builder.Property(b => b.StartTime).IsRequired();
    builder.Property(b => b.StartOdometer).IsRequired();
    builder.Property(b => b.TotalPrice).HasColumnType("decimal(10,2)");
    builder.Property(b => b.IsActive).IsRequired().HasDefaultValue(true);
    builder.HasIndex(b => b.UserId);
    builder.HasIndex(b => b.VehicleId);
    builder.HasIndex(b => b.IsActive);
    builder.HasOne<User>().WithMany().HasForeignKey(b => b.UserId);
    builder.HasOne<Vehicle>().WithMany().HasForeignKey(b => b.VehicleId);
    builder.HasOne<ParkingLot>().WithMany().HasForeignKey(b => b.StartParkingLotId);
  }
}
