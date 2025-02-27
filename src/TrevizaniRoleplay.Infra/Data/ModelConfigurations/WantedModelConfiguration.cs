using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class WantedModelConfiguration : IEntityTypeConfiguration<Wanted>
{
    public void Configure(EntityTypeBuilder<Wanted> builder)
    {
        builder.ToTable("Wanted");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasOne(x => x.WantedCharacter).WithMany().HasForeignKey(x => x.WantedCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.WantedVehicle).WithMany().HasForeignKey(x => x.WantedVehicleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PoliceOfficerCharacter).WithMany().HasForeignKey(x => x.PoliceOfficerCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PoliceOfficerDeletedCharacter).WithMany().HasForeignKey(x => x.PoliceOfficerDeletedCharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}