using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HR_Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "role-admin", null, "Admin", "ADMIN" },
                    { "role-employee", null, "Employee", "EMPLOYEE" },
                    { "role-hr", null, "HR", "HR" },
                    { "role-manager", null, "Manager", "MANAGER" },
                    { "role-recruiter", null, "Recruiter", "RECRUITER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "user-admin", 0, "ab8152df-86ce-46b7-86db-517133b73a20", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@hrplatform.com", true, "Admin", "User", false, null, "ADMIN@HRPLATFORM.COM", "ADMIN@HRPLATFORM.COM", "AQAAAAIAAYagAAAAEEfHlBiNnUOsGDpxWmlmo0Q9iZm6JpLuiNgdn+/Xt7R+KuVluHeuFQtJp4ek0ZD0uA==", null, false, "9b2a6133-d952-4844-bb09-7b5d8a0f9898", false, "admin@hrplatform.com" },
                    { "user-manager", 0, "e7a163e8-619a-4ba9-b077-c566fdff5e14", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "manager@hrplatform.com", true, "Manager", "User", false, null, "MANAGER@HRPLATFORM.COM", "MANAGER@HRPLATFORM.COM", "AQAAAAIAAYagAAAAEL1+e1lHMuUrwPMRKC0Y0xE3joQ2HpzPOuw4rlWyERh/aPpZ/hltdZf3txnEXJyhuA==", null, false, "cfca5c1e-6b5b-4ab7-a7e6-deda32e0baba", false, "manager@hrplatform.com" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "role-admin", "user-admin" },
                    { "role-manager", "user-manager" }
                });

            migrationBuilder.InsertData(
                table: "JobPostings",
                columns: new[] { "Id", "Description", "PostedDate", "RecruiterId", "Title" },
                values: new object[,]
                {
                    { 1, "Develop and maintain web applications.", new DateTime(2024, 11, 26, 15, 21, 11, 255, DateTimeKind.Utc).AddTicks(6980), "user-manager", "Software Developer" },
                    { 2, "Handle recruitment and employee relations.", new DateTime(2024, 11, 26, 15, 21, 11, 255, DateTimeKind.Utc).AddTicks(6984), "user-manager", "HR Specialist" }
                });

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "ManagerId", "Name" },
                values: new object[,]
                {
                    { 1, "user-manager", "Development Team" },
                    { 2, "user-manager", "HR Team" }
                });

            migrationBuilder.InsertData(
                table: "JobApplications",
                columns: new[] { "Id", "ApplicantEmail", "ApplicantName", "JobPostingId", "ResumeUrl", "Status" },
                values: new object[,]
                {
                    { 1, "johndoe@example.com", "John Doe", 1, "https://example.com/resume/johndoe.pdf", "Pending" },
                    { 2, "janesmith@example.com", "Jane Smith", 2, "https://example.com/resume/janesmith.pdf", "Approved" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-employee");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-hr");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-recruiter");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "role-admin", "user-admin" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "role-manager", "user-manager" });

            migrationBuilder.DeleteData(
                table: "JobApplications",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JobApplications",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-admin");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "role-manager");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-admin");

            migrationBuilder.DeleteData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JobPostings",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "user-manager");
        }
    }
}
