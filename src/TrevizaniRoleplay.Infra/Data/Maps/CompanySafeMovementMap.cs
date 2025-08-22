using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class CompanySafeMovementMap : IEntityTypeConfiguration<CompanySafeMovement>
{
    public void Configure(EntityTypeBuilder<CompanySafeMovement> builder)
    {
        builder.ToTable("CompaniesSafesMovements");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Company).WithMany(x => x.SafeMovements).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}