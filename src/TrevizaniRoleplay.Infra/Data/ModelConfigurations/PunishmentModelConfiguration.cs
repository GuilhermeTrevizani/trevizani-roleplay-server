using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PunishmentModelConfiguration : IEntityTypeConfiguration<Punishment>
{
    public void Configure(EntityTypeBuilder<Punishment> builder)
    {
        builder.ToTable("Punishments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.StaffUser).WithMany().HasForeignKey(x => x.StaffUserId).OnDelete(DeleteBehavior.Restrict);
    }
}