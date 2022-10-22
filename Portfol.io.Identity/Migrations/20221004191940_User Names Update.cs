using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfol.io.Identity.Migrations
{
    public partial class UserNamesUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AspNetUsers",
                newName: "MiddleName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "AspNetUsers",
                newName: "Name");

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
    }
}
