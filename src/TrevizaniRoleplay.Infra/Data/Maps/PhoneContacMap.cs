using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class PhoneContacMap : IEntityTypeConfiguration<PhoneContact>
{
    public void Configure(EntityTypeBuilder<PhoneContact> builder)
    {
        builder.ToTable("PhonesContacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(25);
    }
}