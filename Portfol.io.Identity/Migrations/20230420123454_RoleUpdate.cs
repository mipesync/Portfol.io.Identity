using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfol.io.Identity.Migrations
{
    public partial class RoleUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImagePath",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1c2c23c1-2a25-40ac-b084-ae35c259c6c6", "59b14494-428f-4412-aa92-1e35c45af3fc", "support", "SUPPORT" },
                    { "1e78eac6-58b1-4711-89d1-9b6c347220e1", "1bfdfe91-5891-4ec5-ada0-16818bf4da1b", "author", "AUTHOR" },
                    { "2bb164fb-2252-450d-9ad7-aaada543fb74", "d01a9d5f-7ed3-47d2-a05f-4e715bf193ae", "admin", "ADMIN" },
                    { "8e0dcbca-e2cd-45ee-ac80-9cd05b5893d2", "04ba44ec-1ed3-4cdb-8ccf-d20ef98a9d4b", "user", "USER" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1c2c23c1-2a25-40ac-b084-ae35c259c6c6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1e78eac6-58b1-4711-89d1-9b6c347220e1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2bb164fb-2252-450d-9ad7-aaada543fb74");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8e0dcbca-e2cd-45ee-ac80-9cd05b5893d2");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImagePath",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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
    }
}
