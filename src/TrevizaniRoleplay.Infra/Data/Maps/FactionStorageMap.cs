using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class FactionStorageMap : IEntityTypeConfiguration<FactionStorage>
{
    public void Configure(EntityTypeBuilder<FactionStorage> builder)
    {
        builder.ToTable("FactionsStorages");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
    }
}