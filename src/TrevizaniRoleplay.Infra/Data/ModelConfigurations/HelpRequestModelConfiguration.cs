using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class HelpRequestModelConfiguration : IEntityTypeConfiguration<HelpRequest>
{
    public void Configure(EntityTypeBuilder<HelpRequest> builder)
    {
        builder.ToTable("HelpRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(500);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StaffUser).WithMany().HasForeignKey(x => x.StaffUserId).OnDelete(DeleteBehavior.Restrict);
    }
}