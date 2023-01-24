using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfol.io.Identity.Migrations
{
    public partial class RolesUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "600eb0b8-7586-4f39-bcaf-2bab37ca4966");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "734241c6-3b92-4487-b320-5866c3222ee8");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9cde959e-6ef9-4608-8160-65f400c8d157");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "be1c2b09-f748-44ff-8fee-3c040268e71d");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3a4c55f1-d374-4f00-9d6f-3b92be600977", "07c83c75-46da-4191-862f-cc22fcd6e361", "admin", "ADMIN" },
                    { "862bffd0-8c22-44cc-b402-6cd35d9e3c39", "b9976682-968a-44d6-a4dd-9fda0b89048f", "support", "SUPPORT" },
                    { "966fdd07-97af-43e8-a14b-2a23fb3dec66", "fb1aeb4c-a9f1-4967-aa63-f4e2259f049f", "author", "AUTHOR" },
                    { "a482123e-0876-4228-b551-fe7e841a0aad", "6df7a1e0-60f6-4f2d-be08-417c8d959c06", "user", "USER" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3a4c55f1-d374-4f00-9d6f-3b92be600977");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "862bffd0-8c22-44cc-b402-6cd35d9e3c39");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "966fdd07-97af-43e8-a14b-2a23fb3dec66");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a482123e-0876-4228-b551-fe7e841a0aad");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "600eb0b8-7586-4f39-bcaf-2bab37ca4966", "66fe879a-4f8e-4e9a-8d55-9cb3e3320b03", "employer", "EMPLOYER" },
                    { "734241c6-3b92-4487-b320-5866c3222ee8", "f63441c7-1b73-4254-9338-3c7e070e3b9b", "admin", "ADMIN" },
                    { "9cde959e-6ef9-4608-8160-65f400c8d157", "b68cecb5-7925-4c96-a298-ea3cf76ff6fb", "employee", "EMPLOYEE" },
                    { "be1c2b09-f748-44ff-8fee-3c040268e71d", "56a7a19f-eeb6-48b3-a2bf-cbdbc6c88fb2", "support", "SUPPORT" }
                });
        }
    }
}
