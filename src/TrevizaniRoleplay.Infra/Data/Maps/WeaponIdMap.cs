using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class WeaponIdMap : IEntityTypeConfiguration<WeaponId>
{
    public void Configure(EntityTypeBuilder<WeaponId> builder)
    {
        builder.ToTable("WeaponsIds");
        builder.HasKey(x => x.Id);
    }
}