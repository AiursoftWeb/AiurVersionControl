using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SampleWebApp.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InDbEntities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    CommitTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InDbEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InDbEntities_LogItem_ItemId",
                        column: x => x.ItemId,
                        principalTable: "LogItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InDbEntities_ItemId",
                table: "InDbEntities",
                column: "ItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InDbEntities");

            migrationBuilder.DropTable(
                name: "LogItem");
        }
    }
}
