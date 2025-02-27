using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class UserModelConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DiscordId).HasMaxLength(100);
        builder.Property(x => x.DiscordUsername).HasMaxLength(100);
        builder.Property(x => x.DiscordDisplayName).HasMaxLength(100);
        builder.Property(x => x.RegisterIp).HasMaxLength(100);
        builder.Property(x => x.LastAccessIp).HasMaxLength(100);
        builder.Property(x => x.StaffFlagsJSON).HasMaxLength(500);
        builder.Property(x => x.ChatBackgroundColor).HasMaxLength(6);
        builder.Property(x => x.DisplayResolution).HasMaxLength(25);
    }
}