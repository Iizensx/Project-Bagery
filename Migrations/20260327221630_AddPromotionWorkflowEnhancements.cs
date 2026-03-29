using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _66022380.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionWorkflowEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UserId2",
                table: "user_promotion",
                newName: "UserId4");

            migrationBuilder.RenameIndex(
                name: "PromotionId1",
                table: "user_promotion",
                newName: "PromotionId2");

            migrationBuilder.RenameIndex(
                name: "OrderId",
                table: "orderdetail",
                newName: "OrderId1");

            migrationBuilder.RenameIndex(
                name: "UserId1",
                table: "order",
                newName: "UserId2");

            migrationBuilder.RenameIndex(
                name: "UserId",
                table: "historyorder",
                newName: "UserId1");

            migrationBuilder.AddColumn<int>(
                name: "BuyQuantity",
                table: "promotion",
                type: "int",
                nullable: true,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "promotion",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "promotion",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "utf8mb4_0900_ai_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "promotion",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'1'");

            migrationBuilder.AddColumn<bool>(
                name: "IsCombinable",
                table: "promotion",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "MaxUsePerUser",
                table: "promotion",
                type: "int",
                nullable: false,
                defaultValueSql: "'1'");

            migrationBuilder.AddColumn<int>(
                name: "PromoType",
                table: "promotion",
                type: "int",
                nullable: false,
                defaultValueSql: "'1'");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresProof",
                table: "promotion",
                type: "tinyint(1)",
                nullable: false,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<int>(
                name: "RewardProductId",
                table: "promotion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RewardQuantity",
                table: "promotion",
                type: "int",
                nullable: true,
                defaultValueSql: "'0'");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "promotion",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "promotion_claim",
                columns: table => new
                {
                    ClaimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProofImagePath = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "enum('Pending','Approved','Rejected')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewNote = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ClaimId);
                    table.ForeignKey(
                        name: "promotion_claim_ibfk_1",
                        column: x => x.PromotionId,
                        principalTable: "promotion",
                        principalColumn: "PromotionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "promotion_claim_ibfk_2",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "RewardProductId",
                table: "promotion",
                column: "RewardProductId");

            migrationBuilder.CreateIndex(
                name: "PromotionId1",
                table: "promotion_claim",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "ReviewedByUserId",
                table: "promotion_claim",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "UserId3",
                table: "promotion_claim",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "promotion_ibfk_1",
                table: "promotion",
                column: "RewardProductId",
                principalTable: "stock",
                principalColumn: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "promotion_ibfk_1",
                table: "promotion");

            migrationBuilder.DropTable(
                name: "promotion_claim");

            migrationBuilder.DropIndex(
                name: "RewardProductId",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "BuyQuantity",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "IsCombinable",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "MaxUsePerUser",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "PromoType",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "RequiresProof",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "RewardProductId",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "RewardQuantity",
                table: "promotion");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "promotion");

            migrationBuilder.RenameIndex(
                name: "UserId4",
                table: "user_promotion",
                newName: "UserId2");

            migrationBuilder.RenameIndex(
                name: "PromotionId2",
                table: "user_promotion",
                newName: "PromotionId1");

            migrationBuilder.RenameIndex(
                name: "OrderId1",
                table: "orderdetail",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "UserId2",
                table: "order",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "UserId1",
                table: "historyorder",
                newName: "UserId");
        }
    }
}
