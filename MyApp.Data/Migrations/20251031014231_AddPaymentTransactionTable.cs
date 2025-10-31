using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    PrTxId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TxnRef = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_general_ci"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending", collation: "utf8mb4_general_ci"),
                    VnpayTransactionId = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_general_ci"),
                    VnpayResponseCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.PrTxId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_MembershipPackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "MembershipPackages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 42, 31, 338, DateTimeKind.Utc).AddTicks(3071));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 42, 31, 338, DateTimeKind.Utc).AddTicks(3076));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 42, 31, 338, DateTimeKind.Utc).AddTicks(3078));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 42, 31, 338, DateTimeKind.Utc).AddTicks(3079));

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_CreatedAt",
                table: "PaymentTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PackageId",
                table: "PaymentTransactions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TxnRef",
                table: "PaymentTransactions",
                column: "TxnRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_UserId_Status",
                table: "PaymentTransactions",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6104));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6111));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6114));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6116));

            migrationBuilder.InsertData(
                table: "MembershipPackages",
                columns: new[] { "PackageId", "CreatedAt", "Description", "DurationDays", "IsDeleted", "ModelGenerationLimit", "PackageName", "Price", "UpdatedAt" },
                values: new object[] { 5, new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6118), "Gói doanh nghiệp 1 năm với số lượt gen không giới hạn", 365, false, -1, "Enterprise Annual", 4990000m, null });
        }
    }
}
