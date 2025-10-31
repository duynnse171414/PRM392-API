using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentTransactionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VnpayTransactionId",
                table: "PaymentTransactions",
                newName: "VnPayTransactionId");

            migrationBuilder.RenameColumn(
                name: "VnpayResponseCode",
                table: "PaymentTransactions",
                newName: "VnPayResponseCode");

            migrationBuilder.RenameColumn(
                name: "PrTxId",
                table: "PaymentTransactions",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PaymentTransactions",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "TxnRef",
                table: "PaymentTransactions",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_TxnRef",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_OrderId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PaymentTransactions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 49, 7, 612, DateTimeKind.Utc).AddTicks(8315));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 49, 7, 612, DateTimeKind.Utc).AddTicks(8320));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 49, 7, 612, DateTimeKind.Utc).AddTicks(8321));

            migrationBuilder.UpdateData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 1, 49, 7, 612, DateTimeKind.Utc).AddTicks(8359));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PaymentTransactions");

            migrationBuilder.RenameColumn(
                name: "VnPayTransactionId",
                table: "PaymentTransactions",
                newName: "VnpayTransactionId");

            migrationBuilder.RenameColumn(
                name: "VnPayResponseCode",
                table: "PaymentTransactions",
                newName: "VnpayResponseCode");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "PaymentTransactions",
                newName: "PrTxId");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "PaymentTransactions",
                newName: "TxnRef");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "PaymentTransactions",
                newName: "UpdatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_OrderId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_TxnRef");

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
        }
    }
}
