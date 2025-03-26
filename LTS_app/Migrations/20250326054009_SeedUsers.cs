using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LTS_app.Migrations
{
    /// <inheritdoc />
    public partial class SeedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ConfirmationToken", "CreatedAt", "Email", "FullName", "IsActive", "IsConfirmed", "PasswordHash", "ResetPasswordExpiry", "ResetPasswordToken", "Role", "Token", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 3, 25, 14, 31, 30, 0, DateTimeKind.Unspecified), "pachecoking38@gmail.com", "AdminFullName", true, true, "feec62854fa4d276b9e7ca69d4f4d59c7d99017c7a0e680707f454f44cebdbcf", null, "JuvOmbJBtfSk5Jd6AWnXjZdE9pkH0AuXRb1Xanfaovk=", "Admin", "x4lZ0qutV0i8WyGxID5F9DUGHkGV+1WCYkyWThB7WIo=", new DateTime(2025, 3, 15, 6, 38, 6, 0, DateTimeKind.Unspecified), "TestAdmin" },
                    { 2, null, new DateTime(2025, 3, 25, 14, 45, 2, 0, DateTimeKind.Unspecified), "squadquinx8@gmail.com", "UserFullName", true, true, "eb97d409396a3e5392936dad92b909da6f08d8c121a45e1f088fe9768b0c0339", null, "G9Xu424cr46Mw0JfS1y64Ge4MRXp+T9cRcxib+o6Jc8=", "User", "GELmIj47BdHHLt/WvhuIM1EYcWbFkZglNDSv16EcKPw=", new DateTime(2025, 3, 15, 6, 36, 7, 0, DateTimeKind.Unspecified), "TestUser" },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

        }
    }
}
