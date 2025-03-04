﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.Maps;

public class AdminObjectMap : IEntityTypeConfiguration<AdminObject>
{
    public void Configure(EntityTypeBuilder<AdminObject> builder)
    {
        builder.ToTable("AdminObjects");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Model).HasMaxLength(50);
    }
}