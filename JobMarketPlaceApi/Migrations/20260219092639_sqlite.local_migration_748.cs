using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class sqlitelocal_migration_748 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customer_LastName_Id",
                table: "Customer");

            migrationBuilder.CreateTable(
                name: "Contractor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contractor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contractor_Name_Id",
                table: "Contractor",
                columns: new[] { "Name", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contractor");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LastName_Id",
                table: "Customer",
                columns: new[] { "LastName", "Id" });
        }
    }
}
