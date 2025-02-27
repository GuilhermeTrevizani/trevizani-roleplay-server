using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class SessionModelConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Character).WithMany(x => x.Sessions).HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Ip).HasMaxLength(50);
        builder.Property(x => x.SocialClubName).HasMaxLength(50);
    }
}