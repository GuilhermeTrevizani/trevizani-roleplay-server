using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PhoneNumberModelConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder.ToTable("PhonesNumbers");
        builder.HasKey(x => x.Id);
    }
}