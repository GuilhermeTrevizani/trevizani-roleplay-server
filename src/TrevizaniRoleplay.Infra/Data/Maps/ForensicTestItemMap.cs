using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class ForensicTestItemMap : IEntityTypeConfiguration<ForensicTestItem>
{
    public void Configure(EntityTypeBuilder<ForensicTestItem> builder)
    {
        builder.ToTable("ForensicTestsItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Identifier).HasMaxLength(50);
        builder.Property(x => x.Result).HasMaxLength(1000);
        builder.HasOne(x => x.ForensicTest).WithMany(x => x.Items).HasForeignKey(x => x.ForensicTestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OriginConfiscationItem).WithMany().HasForeignKey(x => x.OriginConfiscationItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetConfiscationItem).WithMany().HasForeignKey(x => x.TargetConfiscationItemId).OnDelete(DeleteBehavior.Restrict);
    }
}