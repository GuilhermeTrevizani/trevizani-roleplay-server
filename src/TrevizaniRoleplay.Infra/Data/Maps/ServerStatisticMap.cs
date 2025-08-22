using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class ServerStatisticMap : IEntityTypeConfiguration<ServerStatistic>
{
    public void Configure(EntityTypeBuilder<ServerStatistic> builder)
    {
        builder.ToTable("ServerStatistics");
        builder.HasKey(x => x.Id);
    }
}