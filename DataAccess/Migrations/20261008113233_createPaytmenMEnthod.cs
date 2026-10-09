using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createPaytmenMEnthod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                table: "payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "chek_url",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "confirmed_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "confirmed_by_user_id",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_method",
                table: "payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transaction_note",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amount",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "chek_url",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "confirmed_by_user_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_method",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "transaction_note",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "orders");
        }
    }
}
