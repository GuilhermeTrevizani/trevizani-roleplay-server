using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class FactionUniformModelConfiguration : IEntityTypeConfiguration<FactionUniform>
{
    public void Configure(EntityTypeBuilder<FactionUniform> builder)
    {
        builder.ToTable("FactionsUniforms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
    }
}