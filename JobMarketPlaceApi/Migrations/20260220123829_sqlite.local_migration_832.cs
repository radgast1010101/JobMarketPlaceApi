using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class sqlitelocal_migration_832 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContractorId",
                table: "JobOffer",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractorId",
                table: "JobOffer");
        }
    }
}
