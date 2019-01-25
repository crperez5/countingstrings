using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CountingStrings.Service.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    NumOpen = table.Column<int>(nullable: false),
                    NumClose = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionCounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionCounts");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
