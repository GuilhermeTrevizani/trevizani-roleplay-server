using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class FireModelConfiguration : IEntityTypeConfiguration<Fire>
{
    public void Configure(EntityTypeBuilder<Fire> builder)
    {
        builder.ToTable("Fires");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(100);
    }
}