using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class notificationcompliment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notification_users_user_id",
                table: "notification");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notification",
                table: "notification");

            migrationBuilder.RenameTable(
                name: "notification",
                newName: "notifications");

            migrationBuilder.RenameIndex(
                name: "ix_notification_user_id",
                table: "notifications",
                newName: "ix_notifications_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notifications",
                table: "notifications",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_users_user_id",
                table: "notifications",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notifications_users_user_id",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notifications",
                table: "notifications");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "notification");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_user_id",
                table: "notification",
                newName: "ix_notification_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notification",
                table: "notification",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_notification_users_user_id",
                table: "notification",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
