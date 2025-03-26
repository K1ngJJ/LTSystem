using LTS_app.Models;
using Microsoft.EntityFrameworkCore;

namespace LTS_app.Data
{
    public class DBSeeder
    {
    }
    public class DbSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "TestAdmin",
                    Email = "pachecoking38@gmail.com",
                    PasswordHash = "feec62854fa4d276b9e7ca69d4f4d59c7d99017c7a0e680707f454f44cebdbcf",
                    Role = "Admin",
                    FullName = "AdminFullName",
                    Token = "x4lZ0qutV0i8WyGxID5F9DUGHkGV+1WCYkyWThB7WIo=",
                    IsActive = true,
                    IsConfirmed = true,
                    ConfirmationToken = null,
                    ResetPasswordToken = "JuvOmbJBtfSk5Jd6AWnXjZdE9pkH0AuXRb1Xanfaovk=",
                    ResetPasswordExpiry = null,
                    CreatedAt = DateTime.Parse("2025-03-25 14:31:30"),
                    UpdatedAt = DateTime.Parse("2025-03-15 06:38:06")
                },
                new User
                {
                    Id = 2,
                    Username = "TestUser",
                    Email = "squadquinx8@gmail.com",
                    PasswordHash = "eb97d409396a3e5392936dad92b909da6f08d8c121a45e1f088fe9768b0c0339",
                    Role = "User",
                    FullName = "UserFullName",
                    Token = "GELmIj47BdHHLt/WvhuIM1EYcWbFkZglNDSv16EcKPw=",
                    IsActive = true,
                    IsConfirmed = true,
                    ConfirmationToken = null,
                    ResetPasswordToken = "G9Xu424cr46Mw0JfS1y64Ge4MRXp+T9cRcxib+o6Jc8=",
                    ResetPasswordExpiry = null,
                    CreatedAt = DateTime.Parse("2025-03-25 14:45:02"),
                    UpdatedAt = DateTime.Parse("2025-03-15 06:36:07")
                },
                new User
                {
                    Id = 3,
                    Username = "TestLegislator",
                    Email = "pachecoking57@gmail.com",
                    PasswordHash = "e9e9dd69007f1b63a1aa09c17e6da5319611d1af436b077dfb2300e3caf43b65",
                    Role = "Legislator",
                    FullName = "Legislator FullName",
                    Token = "AITvRemizt/4Dd2ImQdEC/4Gt2OAns+cZRPJ7+Eyz4Y=",
                    IsActive = true,
                    IsConfirmed = true,
                    ConfirmationToken = null,
                    ResetPasswordToken = null,
                    ResetPasswordExpiry = null,
                    CreatedAt = DateTime.Parse("2025-03-15 06:40:49"),
                    UpdatedAt = null
                },
                new User
                {
                    Id = 4,
                    Username = "TestLegislator2",
                    Email = "legislator2@gmail.com",
                    PasswordHash = "e9e9dd69007f1b63a1aa09c17e6da5319611d1af436b077dfb2300e3caf43b65",
                    Role = "Legislator",
                    FullName = "Legislator2 FullName",
                    Token = "M6G7TGnlU7j81rV5FSm06NBIZzYmPq3J206W6OC/+C8=",
                    IsActive = true,
                    IsConfirmed = true,
                    ConfirmationToken = null,
                    ResetPasswordToken = null,
                    ResetPasswordExpiry = null,
                    CreatedAt = DateTime.Parse("2025-03-17 03:05:01"),
                    UpdatedAt = DateTime.Parse("2025-03-17 03:05:10")
                },
                new User
                {
                    Id = 5,
                    Username = "TestLegislator3",
                    Email = "legislator3@gmail.com",
                    PasswordHash = "e9e9dd69007f1b63a1aa09c17e6da5319611d1af436b077dfb2300e3caf43b65",
                    Role = "Legislator",
                    FullName = "Legislator3 FullName",
                    Token = "xp130CmOrvhPxN3Ds68Hdmbz5QKDylBtktURJXZLUiQ=",
                    IsActive = true,
                    IsConfirmed = true,
                    ConfirmationToken = null,
                    ResetPasswordToken = null,
                    ResetPasswordExpiry = null,
                    CreatedAt = DateTime.Parse("2025-03-17 06:21:19"),
                    UpdatedAt = DateTime.Parse("2025-03-17 06:21:29")
                }
            );
        }
    }
}
