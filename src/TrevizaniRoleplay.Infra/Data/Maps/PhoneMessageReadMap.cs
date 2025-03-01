using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class PhoneMessageReadMap : IEntityTypeConfiguration<PhoneMessageRead>
{
    public void Configure(EntityTypeBuilder<PhoneMessageRead> builder)
    {
        builder.ToTable("PhonesMessagesReads");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PhoneMessage).WithMany(x => x.Reads).HasForeignKey(x => x.PhoneMessageId).OnDelete(DeleteBehavior.Restrict);
    }
}