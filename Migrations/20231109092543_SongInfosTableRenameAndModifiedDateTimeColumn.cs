using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSharpWpfShazam.Migrations
{
    /// <inheritdoc />
    public partial class SongInfosTableRenameAndModifiedDateTimeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongInfos",
                table: "SongInfos");

            migrationBuilder.RenameTable(
                name: "SongInfos",
                newName: "SongInfo");

            migrationBuilder.RenameIndex(
                name: "IX_SongInfos_SongUrl",
                table: "SongInfo",
                newName: "IX_SongInfo_SongUrl");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDateTime",
                table: "SongInfo",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongInfo",
                table: "SongInfo",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SongInfo",
                table: "SongInfo");

            migrationBuilder.DropColumn(
                name: "ModifiedDateTime",
                table: "SongInfo");

            migrationBuilder.RenameTable(
                name: "SongInfo",
                newName: "SongInfos");

            migrationBuilder.RenameIndex(
                name: "IX_SongInfo_SongUrl",
                table: "SongInfos",
                newName: "IX_SongInfos_SongUrl");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongInfos",
                table: "SongInfos",
                column: "Id");
        }
    }
}
