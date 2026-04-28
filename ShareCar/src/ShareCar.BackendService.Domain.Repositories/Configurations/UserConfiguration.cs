using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareCar.BackendService.Domain.Models;

namespace ShareCar.BackendService.Domain.Repositories.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("Users");
    builder.HasKey(u => u.Id);
    builder.Property(u => u.Username).HasMaxLength(100).IsRequired();
    builder.HasIndex(u => u.Username).IsUnique();
    builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
    builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
    builder.Property(u => u.Role).IsRequired();
  }
}
