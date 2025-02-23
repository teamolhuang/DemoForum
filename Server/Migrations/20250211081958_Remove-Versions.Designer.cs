﻿// <auto-generated />
using System;
using DemoForum.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DemoForum.Migrations
{
    [DbContext(typeof(ForumContext))]
    [Migration("20250211081958_Remove-Versions")]
    partial class RemoveVersions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("forum")
                .HasAnnotation("ProductVersion", "6.0.36");

            modelBuilder.Entity("DemoForum.Models.Entities.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime");

                    b.Property<int>("PostId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(1)
                        .IsUnicode(false)
                        .HasColumnType("TEXT")
                        .HasComment("P, B, N");

                    b.Property<DateTime?>("UpdatedTime")
                        .HasColumnType("datetime");

                    b.HasKey("Id")
                        .HasName("Comment_pk");

                    b.HasIndex("AuthorId");

                    b.HasIndex(new[] { "Id" }, "Comment_Id_uindex")
                        .IsUnique();

                    b.HasIndex(new[] { "PostId", "CreatedTime" }, "Comment_PostId_CreatedTime_index");

                    b.ToTable("Comment", "forum");
                });

            modelBuilder.Entity("DemoForum.Models.Entities.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CommentScore")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedTime")
                        .HasColumnType("datetime");

                    b.HasKey("Id")
                        .HasName("Post_pk");

                    b.HasIndex("AuthorId");

                    b.ToTable("Post", "forum");
                });

            modelBuilder.Entity("DemoForum.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(72)
                        .IsUnicode(false)
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(12)
                        .IsUnicode(false)
                        .HasColumnType("TEXT");

                    b.HasKey("Id")
                        .HasName("User_pk");

                    b.HasIndex(new[] { "Id" }, "User_Id_uindex")
                        .IsUnique();

                    b.HasIndex(new[] { "Username" }, "User_Username_uindex")
                        .IsUnique();

                    b.ToTable("User", "forum");
                });

            modelBuilder.Entity("DemoForum.Models.Entities.Comment", b =>
                {
                    b.HasOne("DemoForum.Models.Entities.User", "Author")
                        .WithMany("Comments")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Comment_User_Id_fk");

                    b.HasOne("DemoForum.Models.Entities.Post", "Post")
                        .WithMany("Comments")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Comment_Post_Id_fk");

                    b.Navigation("Author");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("DemoForum.Models.Entities.Post", b =>
                {
                    b.HasOne("DemoForum.Models.Entities.User", "Author")
                        .WithMany("Posts")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Post_User_Id_fk");

                    b.Navigation("Author");
                });

            modelBuilder.Entity("DemoForum.Models.Entities.Post", b =>
                {
                    b.Navigation("Comments");
                });

            modelBuilder.Entity("DemoForum.Models.Entities.User", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Posts");
                });
#pragma warning restore 612, 618
        }
    }
}
