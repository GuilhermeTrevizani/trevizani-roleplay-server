using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class FactionUnitCharacterMap : IEntityTypeConfiguration<FactionUnitCharacter>
{
    public void Configure(EntityTypeBuilder<FactionUnitCharacter> builder)
    {
        builder.ToTable("FactionsUnitsCharacters");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.FactionUnit).WithMany(x => x.Characters).HasForeignKey(x => x.FactionUnitId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}