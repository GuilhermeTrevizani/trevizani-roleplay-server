﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrevizaniRoleplay.Domain.Entities;

namespace TrevizaniRoleplay.Infra.Data.ModelConfigurations;

public class PhoneGroupModelConfiguration : IEntityTypeConfiguration<PhoneGroup>
{
    public void Configure(EntityTypeBuilder<PhoneGroup> builder)
    {
        builder.ToTable("PhonesGroups");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(25);
    }
}