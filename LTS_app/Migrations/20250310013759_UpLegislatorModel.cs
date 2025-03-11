using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTS_app.Migrations
{
    /// <inheritdoc />
    public partial class UpLegislatorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Legislators",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Legislators",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Legislators");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Legislators");
        }
    }
}
