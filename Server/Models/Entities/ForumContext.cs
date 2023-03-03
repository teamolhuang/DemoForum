﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DemoForum.Models.Entities;

public partial class ForumContext : DbContext
{
    public ForumContext()
    {
    }

    public ForumContext(DbContextOptions<ForumContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:DemoForum");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("forum");

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Comment_pk");

            entity.ToTable("Comment");

            entity.HasIndex(e => e.Id, "Comment_Id_uindex").IsUnique();

            entity.HasIndex(e => new { e.PostId, e.CreatedTime }, "Comment_PostId_CreatedTime_index");

            entity.Property(e => e.Content)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

            entity.HasOne(d => d.Author).WithMany(p => p.Comments)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Comment_User_Id_fk");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Comment_Post_Id_fk");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Post_pk");

            entity.ToTable("Post");

            entity.Property(e => e.Content).HasMaxLength(2000);
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(20);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Author).WithMany(p => p.Posts)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Post_User_Id_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pk");

            entity.ToTable("User");

            entity.HasIndex(e => e.Id, "User_Id_uindex").IsUnique();

            entity.HasIndex(e => e.Username, "User_Username_uindex").IsUnique();

            entity.Property(e => e.Password)
                .HasMaxLength(72)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(12)
                .IsUnicode(false);
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
