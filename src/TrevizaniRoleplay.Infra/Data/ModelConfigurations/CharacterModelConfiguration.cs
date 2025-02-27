using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class CharacterModelConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("Characters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.RegisterIp).HasMaxLength(50);
        builder.Property(x => x.LastAccessIp).HasMaxLength(50);
        builder.Property(x => x.Attributes).HasMaxLength(500);
        builder.HasOne(x => x.User).WithMany(x => x.Characters).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Faction).WithMany().HasForeignKey(x => x.FactionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FactionRank).WithMany().HasForeignKey(x => x.FactionRankId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EvaluatingStaffUser).WithMany().HasForeignKey(x => x.EvaluatingStaffUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EvaluatorStaffUser).WithMany().HasForeignKey(x => x.EvaluatorStaffUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PoliceOfficerBlockedDriverLicenseCharacter).WithMany().HasForeignKey(x => x.PoliceOfficerBlockedDriverLicenseCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PoliceOfficerWeaponLicenseCharacter).WithMany().HasForeignKey(x => x.PoliceOfficerWeaponLicenseCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DrugItemTemplate).WithMany().HasForeignKey(x => x.DrugItemTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}