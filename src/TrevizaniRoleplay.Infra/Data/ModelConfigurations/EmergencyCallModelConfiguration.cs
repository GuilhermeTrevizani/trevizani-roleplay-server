using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class EmergencyCallModelConfiguration : IEntityTypeConfiguration<EmergencyCall>
{
    public void Configure(EntityTypeBuilder<EmergencyCall> builder)
    {
        builder.ToTable("EmergencyCalls");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(500);
        builder.Property(x => x.Location).HasMaxLength(500);
        builder.Property(x => x.PosLocation).HasMaxLength(500);
    }
}