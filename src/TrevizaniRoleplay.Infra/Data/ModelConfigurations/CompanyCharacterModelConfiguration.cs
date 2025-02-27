using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class CompanyCharacterModelConfiguration : IEntityTypeConfiguration<CompanyCharacter>
{
    public void Configure(EntityTypeBuilder<CompanyCharacter> builder)
    {
        builder.ToTable("CompaniesCharacters");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Company).WithMany(x => x.Characters).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}