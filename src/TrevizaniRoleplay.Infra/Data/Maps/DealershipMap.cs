using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class DealershipMap : IEntityTypeConfiguration<Dealership>
{
    public void Configure(EntityTypeBuilder<Dealership> builder)
    {
        builder.ToTable("Dealerships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50);
    }
}