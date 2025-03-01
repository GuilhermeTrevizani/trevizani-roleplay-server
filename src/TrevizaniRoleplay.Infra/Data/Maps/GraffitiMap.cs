using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class GraffitiMap : IEntityTypeConfiguration<Graffiti>
{
    public void Configure(EntityTypeBuilder<Graffiti> builder)
    {
        builder.ToTable("Graffitis");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(35);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}