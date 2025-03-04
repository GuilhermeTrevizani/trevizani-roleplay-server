﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class BodyMap : IEntityTypeConfiguration<Body>
{
    public void Configure(EntityTypeBuilder<Body> builder)
    {
        builder.ToTable("Bodies");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Character).WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Restrict);
    }
}