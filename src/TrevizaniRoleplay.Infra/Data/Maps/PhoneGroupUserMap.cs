using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class PhoneGroupUserMap : IEntityTypeConfiguration<PhoneGroupUser>
{
    public void Configure(EntityTypeBuilder<PhoneGroupUser> builder)
    {
        builder.ToTable("PhonesGroupsUsers");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PhoneGroup).WithMany(x => x.Users).HasForeignKey(x => x.PhoneGroupId).OnDelete(DeleteBehavior.Restrict);
    }
}