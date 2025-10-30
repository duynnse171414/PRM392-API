using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipSubscriptionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPackage");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PackageName",
                table: "MembershipPackages",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MembershipPackages",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MembershipPackages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ModelGenerationLimit",
                table: "MembershipPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MembershipPackages",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserMembershipSubscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_general_ci"),
                    RemainingGenerations = table.Column<int>(type: "int", nullable: false),
                    TotalGenerationsUsed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMembershipSubscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_UserMembershipSubscriptions_MembershipPackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "MembershipPackages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMembershipSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.InsertData(
                table: "MembershipPackages",
                columns: new[] { "PackageId", "CreatedAt", "Description", "DurationDays", "IsDeleted", "ModelGenerationLimit", "PackageName", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6104), "Gói dùng thử miễn phí với 5 lượt gen model 3D", 7, false, 5, "Free Trial", 0m, null },
                    { 2, new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6111), "Gói cơ bản cho người dùng thông thường với 50 lượt gen/tháng", 30, false, 50, "Basic", 99000m, null },
                    { 3, new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6114), "Gói cao cấp với 200 lượt gen/tháng và ưu tiên hỗ trợ", 30, false, 200, "Premium", 299000m, null },
                    { 4, new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6116), "Gói chuyên nghiệp với số lượt gen không giới hạn", 30, false, -1, "Pro", 499000m, null },
                    { 5, new DateTime(2025, 10, 30, 7, 10, 9, 845, DateTimeKind.Utc).AddTicks(6118), "Gói doanh nghiệp 1 năm với số lượt gen không giới hạn", 365, false, -1, "Enterprise Annual", 4990000m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPackages_PackageName",
                table: "MembershipPackages",
                column: "PackageName");

            migrationBuilder.CreateIndex(
                name: "IX_UserMembershipSubscriptions_PackageId",
                table: "UserMembershipSubscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMembershipSubscriptions_Status",
                table: "UserMembershipSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserMembershipSubscriptions_UserId_Status_EndDate",
                table: "UserMembershipSubscriptions",
                columns: new[] { "UserId", "Status", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMembershipSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_MembershipPackages_PackageName",
                table: "MembershipPackages");

            migrationBuilder.DeleteData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MembershipPackages",
                keyColumn: "PackageId",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "ModelGenerationLimit",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MembershipPackages");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "PackageName",
                table: "MembershipPackages",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MembershipPackages",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_general_ci",
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateTable(
                name: "UserPackage",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPackage", x => new { x.UserId, x.PackageId });
                    table.ForeignKey(
                        name: "FK_UserPackage_MembershipPackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "MembershipPackages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPackage_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "utf8mb4_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_UserPackage_PackageId",
                table: "UserPackage",
                column: "PackageId");
        }
    }
}
