using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfScheduler.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitlistEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "waitlist_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tee_time_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waitlist_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_waitlist_entries_tee_times_tee_time_id",
                        column: x => x.tee_time_id,
                        principalTable: "tee_times",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_waitlist_entries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_tee_time_id_user_id",
                table: "waitlist_entries",
                columns: new[] { "tee_time_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_waitlist_entries_user_id",
                table: "waitlist_entries",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "waitlist_entries");
        }
    }
}
