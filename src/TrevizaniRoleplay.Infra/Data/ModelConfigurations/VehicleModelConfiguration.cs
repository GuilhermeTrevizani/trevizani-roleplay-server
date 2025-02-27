using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class VehicleModelConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Model).HasMaxLength(25);
        builder.Property(x => x.Plate).HasMaxLength(8);
        builder.Property(x => x.ExtrasJSON).HasMaxLength(200);
        builder.Property(x => x.NewPlate).HasMaxLength(8);
        builder.Property(x => x.Description).HasMaxLength(100);
        builder.HasOne(x => x.Character).WithMany(x => x.Vehicles).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
    }
}