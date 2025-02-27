﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class TruckerLocationDeliveryModelConfiguration : IEntityTypeConfiguration<TruckerLocationDelivery>
{
    public void Configure(EntityTypeBuilder<TruckerLocationDelivery> builder)
    {
        builder.ToTable("TruckerLocationsDeliveries");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.TruckerLocation).WithMany().HasForeignKey(x => x.TruckerLocationId).OnDelete(DeleteBehavior.Restrict);
    }
}