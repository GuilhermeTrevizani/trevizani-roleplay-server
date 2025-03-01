using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class UCPActionExecutedMap : IEntityTypeConfiguration<UCPActionExecuted>
{
    public void Configure(EntityTypeBuilder<UCPActionExecuted> builder)
    {
        builder.ToTable("UCPActionsExecuted");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}