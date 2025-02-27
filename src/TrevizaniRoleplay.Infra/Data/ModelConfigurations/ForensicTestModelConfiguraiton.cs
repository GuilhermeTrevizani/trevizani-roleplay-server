using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class ForensicTestModelConfiguration : IEntityTypeConfiguration<ForensicTest>
{
    public void Configure(EntityTypeBuilder<ForensicTest> builder)
    {
        builder.ToTable("ForensicTests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Identifier).HasMaxLength(50);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
    }
}