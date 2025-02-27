using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class CompanyTuningPriceModelConfiguration : IEntityTypeConfiguration<CompanyTuningPrice>
{
    public void Configure(EntityTypeBuilder<CompanyTuningPrice> builder)
    {
        builder.ToTable("CompaniesTuningPrices");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Company).WithMany(x => x.TuningPrices).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}