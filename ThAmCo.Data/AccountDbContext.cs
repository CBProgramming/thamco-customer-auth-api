﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace ThAmCo.Data
{
    public class AccountDbContext : IdentityDbContext<AppUser>
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("account");

            builder.Entity<AppUser>()
                .Property(a => a.CustomerId)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Descriptor = "ThAmCo Administrators"
                },
                new AppRole
                {
                    Name = "Staff",
                    NormalizedName = "STAFF",
                    Descriptor = "ThAmCo Staff Members"
                },
                new AppRole
                {
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    Descriptor = "ThAmCo Customers"
                }
            );
        }
    }
}
