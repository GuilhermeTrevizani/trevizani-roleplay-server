using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class CrackDenModelConfiguration : IEntityTypeConfiguration<CrackDen>
{
    public void Configure(EntityTypeBuilder<CrackDen> builder)
    {
        builder.ToTable("CrackDens");
        builder.HasKey(x => x.Id);
    }
}