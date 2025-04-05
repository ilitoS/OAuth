using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OAuth.Server.Models;

public partial class OauthAppContext : DbContext
{
    public OauthAppContext()
    {
    }

    public OauthAppContext(DbContextOptions<OauthAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host={value};Database={value};Username={value};Password={value}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.Property(e => e.AuthType).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(250);
            entity.Property(e => e.Username).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
