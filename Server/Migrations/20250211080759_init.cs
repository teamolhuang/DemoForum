using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoForum.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "forum");

            migrationBuilder.CreateTable(
                name: "User",
                schema: "forum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", unicode: false, maxLength: 12, nullable: false),
                    Password = table.Column<string>(type: "TEXT", unicode: false, maxLength: 72, nullable: false),
                    Version = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                schema: "forum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    CommentScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Post_pk", x => x.Id);
                    table.ForeignKey(
                        name: "Post_User_Id_fk",
                        column: x => x.AuthorId,
                        principalSchema: "forum",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                schema: "forum",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PostId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    Type = table.Column<string>(type: "TEXT", unicode: false, maxLength: 1, nullable: false, comment: "P, B, N")
                },
                constraints: table =>
                {
                    table.PrimaryKey("Comment_pk", x => x.Id);
                    table.ForeignKey(
                        name: "Comment_Post_Id_fk",
                        column: x => x.PostId,
                        principalSchema: "forum",
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "Comment_User_Id_fk",
                        column: x => x.AuthorId,
                        principalSchema: "forum",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "Comment_Id_uindex",
                schema: "forum",
                table: "Comment",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Comment_PostId_CreatedTime_index",
                schema: "forum",
                table: "Comment",
                columns: new[] { "PostId", "CreatedTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AuthorId",
                schema: "forum",
                table: "Comment",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_AuthorId",
                schema: "forum",
                table: "Post",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "User_Id_uindex",
                schema: "forum",
                table: "User",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "User_Username_uindex",
                schema: "forum",
                table: "User",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comment",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "Post",
                schema: "forum");

            migrationBuilder.DropTable(
                name: "User",
                schema: "forum");
        }
    }
}
