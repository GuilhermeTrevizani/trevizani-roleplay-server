using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class BodyItemModelConfiguration : IEntityTypeConfiguration<BodyItem>
{
    public void Configure(EntityTypeBuilder<BodyItem> builder)
    {
        builder.ToTable("BodiesItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Body).WithMany(x => x.Items).HasForeignKey(x => x.BodyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}