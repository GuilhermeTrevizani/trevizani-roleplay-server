using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class DealershipVehicleModelConfiguration : IEntityTypeConfiguration<DealershipVehicle>
{
    public void Configure(EntityTypeBuilder<DealershipVehicle> builder)
    {
        builder.ToTable("DealershipsVehicles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Model).HasMaxLength(50);
        builder.HasOne(x => x.Dealership).WithMany().HasForeignKey(x => x.DealershipId);
    }
}