using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class FactionModelConfiguration : IEntityTypeConfiguration<Faction>
{
    public void Configure(EntityTypeBuilder<Faction> builder)
    {
        builder.ToTable("Factions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.Property(x => x.Color).HasMaxLength(6);
        builder.Property(x => x.ChatColor).HasMaxLength(6);
        builder.HasOne(x => x.Character).WithOne().HasForeignKey<Faction>(x => x.CharacterId);
    }
}