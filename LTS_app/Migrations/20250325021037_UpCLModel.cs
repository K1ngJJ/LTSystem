using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTS_app.Migrations
{
    /// <inheritdoc />
    public partial class UpCLModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "CommitteeLegislators",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "CommitteeLegislators");
        }
    }
}
