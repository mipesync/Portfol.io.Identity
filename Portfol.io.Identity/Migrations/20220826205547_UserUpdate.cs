using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfol.io.Identity.Migrations
{
    public partial class UserUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "05281d74-4e38-40c2-981a-0e7d6493b97c", "a3a6d9c8-b3a3-49d7-a966-3dd6857f672b", "admin", "ADMIN" },
                    { "aa1d0194-1708-4422-9fbf-a69d3c1d3cc1", "f589e71e-7528-4074-b1de-b64cbc715eb9", "employer", "EMPLOYER" },
                    { "aea5757c-5d42-47b4-a690-8aad1b7731a9", "aedced92-b638-418b-9fac-c0e653bf3122", "support", "SUPPORT" },
                    { "eff8318f-d545-4c06-8bfa-3fbd6ca545e9", "3e927698-6c02-4bb1-b668-0b0c00074348", "employee", "EMPLOYEE" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "05281d74-4e38-40c2-981a-0e7d6493b97c");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "aa1d0194-1708-4422-9fbf-a69d3c1d3cc1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "aea5757c-5d42-47b4-a690-8aad1b7731a9");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "eff8318f-d545-4c06-8bfa-3fbd6ca545e9");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "AspNetUsers");
        }
    }
}
