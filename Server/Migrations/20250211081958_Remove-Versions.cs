using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoForum.Migrations
{
    public partial class RemoveVersions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "forum",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "forum",
                table: "Post");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "forum",
                table: "User",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                schema: "forum",
                table: "Post",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
