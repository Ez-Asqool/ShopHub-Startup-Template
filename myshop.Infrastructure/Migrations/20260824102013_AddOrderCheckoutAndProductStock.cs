using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myshop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCheckoutAndProductStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Carrier",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "ShippingDate",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "TrakcingNumber",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "OrderHeaders",
                newName: "RecipientName");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "OrderDetails",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "Count",
                table: "OrderDetails",
                newName: "Quantity");

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "OrderHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrderStatus",
                table: "OrderHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "RecipientName",
                table: "OrderHeaders",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderDetails",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderDetails",
                newName: "Count");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "OrderHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "OrderStatus",
                table: "OrderHeaders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Carrier",
                table: "OrderHeaders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "OrderHeaders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "OrderHeaders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeaders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippingDate",
                table: "OrderHeaders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TrakcingNumber",
                table: "OrderHeaders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
