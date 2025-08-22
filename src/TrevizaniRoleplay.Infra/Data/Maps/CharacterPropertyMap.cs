using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CharacterPropertyMap : IEntityTypeConfiguration<CharacterProperty>
{
    public void Configure(EntityTypeBuilder<CharacterProperty> builder)
    {
        builder.ToTable("CharactersProperties");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Character).WithMany(x => x.PropertiesAccess).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Property).WithMany(x => x.CharactersAccess).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
    }
}