using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfScheduler.Api.Migrations
{
    /// <inheritdoc />
    public partial class WaitlistDayLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_waitlist_entries_tee_times_tee_time_id",
                table: "waitlist_entries");

            migrationBuilder.DropIndex(
                name: "IX_waitlist_entries_tee_time_id_user_id",
                table: "waitlist_entries");

            migrationBuilder.DropColumn(
                name: "tee_time_id",
                table: "waitlist_entries");

            migrationBuilder.AddColumn<DateOnly>(
                name: "tee_date",
                table: "waitlist_entries",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_tee_date_user_id",
                table: "waitlist_entries",
                columns: new[] { "tee_date", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_waitlist_entries_tee_date_user_id",
                table: "waitlist_entries");

            migrationBuilder.DropColumn(
                name: "tee_date",
                table: "waitlist_entries");

            migrationBuilder.AddColumn<Guid>(
                name: "tee_time_id",
                table: "waitlist_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_tee_time_id_user_id",
                table: "waitlist_entries",
                columns: new[] { "tee_time_id", "user_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_waitlist_entries_tee_times_tee_time_id",
                table: "waitlist_entries",
                column: "tee_time_id",
                principalTable: "tee_times",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
