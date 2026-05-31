using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryPro.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "101, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ReorderLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.CheckConstraint("CK_Products_Quantity", "[QuantityInStock] >= 0");
                    table.CheckConstraint("CK_Products_ReorderLevel", "[ReorderLevel] >= 0");
                    table.CheckConstraint("CK_Products_UnitPrice", "[UnitPrice] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    SaleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1001, 1"),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    QuantitySold = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.SaleID);
                    table.CheckConstraint("CK_Sales_QuantitySold", "[QuantitySold] > 0");
                    table.CheckConstraint("CK_Sales_TotalPrice", "[TotalPrice] > 0");
                    table.ForeignKey(
                        name: "FK_Sales_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sales_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductID", "Description", "LastUpdated", "ProductName", "QuantityInStock", "ReorderLevel", "Supplier", "UnitPrice" },
                values: new object[,]
                {
                    { 101, "White A4 printing paper, 80gsm", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A4 Paper (500 sheets)", 150, 20, "PaperWorld Durban", 45.00m },
                    { 102, "Blue ballpoint pens, medium tip", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ballpoint Pens (Box of 10)", 8, 10, "Stationery Hub", 25.00m },
                    { 103, "Steel 4-drawer filing cabinet, grey", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Filing Cabinet (4-drawer)", 3, 5, "Office Essentials SA", 850.00m },
                    { 104, "Heavy-duty stapler with staples", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Stapler", 25, 8, "Stationery Hub", 55.00m },
                    { 105, "Assorted colours: red, blue, green, black", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Whiteboard Markers (Set of 4)", 4, 10, "PaperWorld Durban", 35.00m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedDate", "FullName", "LastLoginDate", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lefa Mokoena", null, "$2a$11$IAkvyWAKZwhhW5qGbX3IGuHZ9STPzYqvBC94Cb1ACbE8saz8BQXmu", "Manager", "lefa_m" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kago Khumalo", null, "$2a$11$vaowO8s7XhKTHyyksKd2t.sBDUcQGzsVPIIjbnRLWth93OwfObf6C", "Shop Assistant", "kago_k" }
                });

            migrationBuilder.InsertData(
                table: "Sales",
                columns: new[] { "SaleID", "ProductID", "QuantitySold", "SaleDate", "TotalPrice", "UserID" },
                values: new object[,]
                {
                    { 1001, 101, 5, new DateTime(2025, 1, 10, 9, 30, 0, 0, DateTimeKind.Unspecified), 225.00m, 2 },
                    { 1002, 102, 3, new DateTime(2025, 1, 10, 11, 0, 0, 0, DateTimeKind.Unspecified), 75.00m, 2 },
                    { 1003, 103, 1, new DateTime(2025, 1, 11, 14, 0, 0, 0, DateTimeKind.Unspecified), 850.00m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_ProductID",
                table: "Sales",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_UserID",
                table: "Sales",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
