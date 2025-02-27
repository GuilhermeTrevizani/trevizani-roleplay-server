using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PhoneCallModelConfiguration : IEntityTypeConfiguration<PhoneCall>
{
    public void Configure(EntityTypeBuilder<PhoneCall> builder)
    {
        builder.ToTable("PhonesCalls");
        builder.HasKey(x => x.Id);
    }
}