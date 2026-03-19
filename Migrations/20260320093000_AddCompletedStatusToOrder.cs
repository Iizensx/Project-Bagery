using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _66022380.Migrations
{
    public partial class AddCompletedStatusToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "order",
                type: "enum('Pending','Paid','Preparing','Shipped','Completed','Cancelled')",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "enum('Pending','Paid','Preparing','Shipped','Cancelled')",
                oldNullable: true)
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE `order` SET `Status` = 'Shipped' WHERE `Status` = 'Completed';");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "order",
                type: "enum('Pending','Paid','Preparing','Shipped','Cancelled')",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "enum('Pending','Paid','Preparing','Shipped','Completed','Cancelled')",
                oldNullable: true)
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");
        }
    }
}
