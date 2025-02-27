using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class CrackDenItemModelConfiguration : IEntityTypeConfiguration<CrackDenItem>
{
    public void Configure(EntityTypeBuilder<CrackDenItem> builder)
    {
        builder.ToTable("CrackDensItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.CrackDen).WithMany().HasForeignKey(x => x.CrackDenId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}