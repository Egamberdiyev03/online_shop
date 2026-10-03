using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_carts_customers_customer_id",
                table: "carts");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_customers_customer_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_customers_customer_id",
                table: "orders");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "orders",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_orders_customer_id",
                table: "orders",
                newName: "ix_orders_user_id");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "comments",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_comments_customer_id",
                table: "comments",
                newName: "ix_comments_user_id");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "carts",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_carts_customer_id",
                table: "carts",
                newName: "ix_carts_user_id");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_carts_users_user_id",
                table: "carts",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_comments_users_user_id",
                table: "comments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_carts_users_user_id",
                table: "carts");

            migrationBuilder.DropForeignKey(
                name: "fk_comments_users_user_id",
                table: "comments");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_user_id",
                table: "orders");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "orders",
                newName: "customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_orders_user_id",
                table: "orders",
                newName: "ix_orders_customer_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "comments",
                newName: "customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_comments_user_id",
                table: "comments",
                newName: "ix_comments_customer_id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "carts",
                newName: "customer_id");

            migrationBuilder.RenameIndex(
                name: "ix_carts_user_id",
                table: "carts",
                newName: "ix_carts_customer_id");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "fk_carts_customers_customer_id",
                table: "carts",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_comments_customers_customer_id",
                table: "comments",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_customers_customer_id",
                table: "orders",
                column: "customer_id",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
