using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfol.io.Identity.Migrations
{
    public partial class UserUpdate_DateOfBirth_type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4d26c762-fd2e-478f-a918-48a76ef0fe9a", "d0352e38-e1e9-4642-b1ce-3b00c66a214d", "employer", "EMPLOYER" },
                    { "870953de-9d0d-42de-8950-d1cd2495571f", "c4eca907-ea1e-48fd-87e1-4a458f590ff1", "admin", "ADMIN" },
                    { "b61e218e-fa5d-4d4e-bf31-1ef074b82dfd", "4109596c-c624-4f78-bb77-4afc14421110", "support", "SUPPORT" },
                    { "f8f64f7d-56f5-4f33-b420-02819ce2ed6f", "21a7d3b9-d733-4054-9720-7a21459d1e1d", "employee", "EMPLOYEE" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4d26c762-fd2e-478f-a918-48a76ef0fe9a");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "870953de-9d0d-42de-8950-d1cd2495571f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "b61e218e-fa5d-4d4e-bf31-1ef074b82dfd");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f8f64f7d-56f5-4f33-b420-02819ce2ed6f");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

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
    }
}
