using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CharacterItemMap : IEntityTypeConfiguration<CharacterItem>
{
    public void Configure(EntityTypeBuilder<CharacterItem> builder)
    {
        builder.ToTable("CharactersItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Character).WithMany(x => x.Items).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}