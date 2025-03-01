using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class ConfiscationItemMap : IEntityTypeConfiguration<ConfiscationItem>
{
    public void Configure(EntityTypeBuilder<ConfiscationItem> builder)
    {
        builder.ToTable("ConfiscationsItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Identifier).HasMaxLength(50);
        builder.HasOne(x => x.Confiscation).WithMany(x => x.Items).HasForeignKey(x => x.ConfiscationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}