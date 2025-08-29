using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class InfoMap : IEntityTypeConfiguration<Info>
{
    public void Configure(EntityTypeBuilder<Info> builder)
    {
        builder.ToTable("Infos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(500);
        builder.Property(x => x.Image).HasMaxLength(500);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}