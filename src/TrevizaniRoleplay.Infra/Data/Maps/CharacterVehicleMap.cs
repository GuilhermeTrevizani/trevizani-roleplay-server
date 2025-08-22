using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CharacterVehicleMap : IEntityTypeConfiguration<CharacterVehicle>
{
    public void Configure(EntityTypeBuilder<CharacterVehicle> builder)
    {
        builder.ToTable("CharactersVehicles");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Character).WithMany(x => x.VehiclesAccess).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Vehicle).WithMany(x => x.CharactersAccess).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
    }
}