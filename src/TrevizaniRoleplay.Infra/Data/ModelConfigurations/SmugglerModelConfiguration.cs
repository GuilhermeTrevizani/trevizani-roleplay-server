using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class SmugglerModelConfiguration : IEntityTypeConfiguration<Smuggler>
{
    public void Configure(EntityTypeBuilder<Smuggler> builder)
    {
        builder.ToTable("Smugglers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Model).HasMaxLength(100);
    }
}