using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class SeizedVehicleMap : IEntityTypeConfiguration<SeizedVehicle>
{
    public void Configure(EntityTypeBuilder<SeizedVehicle> builder)
    {
        builder.ToTable("SeizedVehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(255);
        builder.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PoliceOfficerCharacter).WithMany().HasForeignKey(x => x.PoliceOfficerCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
    }
}