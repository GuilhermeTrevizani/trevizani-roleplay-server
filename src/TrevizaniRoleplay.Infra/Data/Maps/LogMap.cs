using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class LogMap : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("Logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OriginIp).HasMaxLength(50);
        builder.Property(x => x.OriginSocialClubName).HasMaxLength(50);
        builder.Property(x => x.TargetIp).HasMaxLength(50);
        builder.Property(x => x.TargetSocialClubName).HasMaxLength(50);
        builder.HasOne(x => x.OriginCharacter).WithMany().HasForeignKey(x => x.OriginCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.TargetCharacter).WithMany().HasForeignKey(x => x.TargetCharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.OriginUser).WithMany().HasForeignKey(x => x.OriginUserId).OnDelete(DeleteBehavior.Restrict);
    }
}