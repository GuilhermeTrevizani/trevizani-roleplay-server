using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PhoneMessageModelConfiguration : IEntityTypeConfiguration<PhoneMessage>
{
    public void Configure(EntityTypeBuilder<PhoneMessage> builder)
    {
        builder.ToTable("PhonesMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(255);
        builder.HasOne(x => x.PhoneGroup).WithMany().HasForeignKey(x => x.PhoneGroupId).OnDelete(DeleteBehavior.Restrict);
    }
}