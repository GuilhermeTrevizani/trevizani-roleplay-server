using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CrackDenSellMap : IEntityTypeConfiguration<CrackDenSell>
{
    public void Configure(EntityTypeBuilder<CrackDenSell> builder)
    {
        builder.ToTable("CrackDensSells");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.CrackDen).WithMany().HasForeignKey(x => x.CrackDenId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}