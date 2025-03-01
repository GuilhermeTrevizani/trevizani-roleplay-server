using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class DrugMap : IEntityTypeConfiguration<Drug>
{
    public void Configure(EntityTypeBuilder<Drug> builder)
    {
        builder.ToTable("Drugs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Warn).HasMaxLength(300);
        builder.Property(x => x.ShakeGameplayCamName).HasMaxLength(100);
        builder.Property(x => x.TimecycModifier).HasMaxLength(100);
        builder.Property(x => x.AnimpostFXName).HasMaxLength(100);
        builder.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}