using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CrimeMap : IEntityTypeConfiguration<Crime>
{
    public void Configure(EntityTypeBuilder<Crime> builder)
    {
        builder.ToTable("Crimes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100);
    }
}