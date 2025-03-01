using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class PremiumPointPurchaseMap : IEntityTypeConfiguration<PremiumPointPurchase>
{
    public void Configure(EntityTypeBuilder<PremiumPointPurchase> builder)
    {
        builder.ToTable("PremiumPointPurchases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Status).HasMaxLength(100);
        builder.Property(x => x.PreferenceId).HasMaxLength(200);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetUser).WithMany().HasForeignKey(x => x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
    }
}