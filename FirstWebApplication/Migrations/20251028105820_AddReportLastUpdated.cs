using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddReportLastUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Reports",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Reports");
        }
    }
}
