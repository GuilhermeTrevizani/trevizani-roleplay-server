using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class PhoneCallMap : IEntityTypeConfiguration<PhoneCall>
{
    public void Configure(EntityTypeBuilder<PhoneCall> builder)
    {
        builder.ToTable("PhonesCalls");
        builder.HasKey(x => x.Id);
    }
}