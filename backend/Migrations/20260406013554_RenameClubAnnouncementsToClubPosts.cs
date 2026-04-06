using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameClubAnnouncementsToClubPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ClubAnnouncements",
                newName: "ClubPosts");

            migrationBuilder.RenameIndex(
                name: "IX_ClubAnnouncements_UserId",
                table: "ClubPosts",
                newName: "IX_ClubPosts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ClubAnnouncements_ClubId",
                table: "ClubPosts",
                newName: "IX_ClubPosts_ClubId");

            migrationBuilder.AddColumn<int>(
                name: "PostType",
                table: "ClubPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikesCount",
                table: "ClubPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "ClubPosts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostType",
                table: "ClubPosts");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "ClubPosts");

            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "ClubPosts");

            migrationBuilder.RenameIndex(
                name: "IX_ClubPosts_UserId",
                table: "ClubAnnouncements",
                newName: "IX_ClubAnnouncements_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ClubPosts_ClubId",
                table: "ClubAnnouncements",
                newName: "IX_ClubAnnouncements_ClubId");

            migrationBuilder.RenameTable(
                name: "ClubPosts",
                newName: "ClubAnnouncements");
        }
    }
}
