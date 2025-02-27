using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class WeaponIdModelConfiguration : IEntityTypeConfiguration<WeaponId>
{
    public void Configure(EntityTypeBuilder<WeaponId> builder)
    {
        builder.ToTable("WeaponsIds");
        builder.HasKey(x => x.Id);
    }
}