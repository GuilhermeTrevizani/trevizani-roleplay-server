using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CompanyItemMap : IEntityTypeConfiguration<CompanyItem>
{
    public void Configure(EntityTypeBuilder<CompanyItem> builder)
    {
        builder.ToTable("CompaniesItems");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Company).WithMany(x => x.Items).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}