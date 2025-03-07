using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTS_app.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusBillHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BillHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BillHistories");
        }
    }
}
