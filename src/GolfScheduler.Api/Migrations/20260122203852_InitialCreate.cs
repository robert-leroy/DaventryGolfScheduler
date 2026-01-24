using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfScheduler.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    azure_ad_b2c_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tee_times",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tee_date = table.Column<DateOnly>(type: "date", nullable: false),
                    tee_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    max_players = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tee_times", x => x.id);
                    table.ForeignKey(
                        name: "FK_tee_times_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "registrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tee_time_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registrations", x => x.id);
                    table.ForeignKey(
                        name: "FK_registrations_tee_times_tee_time_id",
                        column: x => x.tee_time_id,
                        principalTable: "tee_times",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registrations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_registrations_tee_time_id_user_id",
                table: "registrations",
                columns: new[] { "tee_time_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_registrations_user_id",
                table: "registrations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tee_times_created_by",
                table: "tee_times",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tee_times_tee_date_tee_time",
                table: "tee_times",
                columns: new[] { "tee_date", "tee_time" });

            migrationBuilder.CreateIndex(
                name: "IX_users_azure_ad_b2c_id",
                table: "users",
                column: "azure_ad_b2c_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registrations");

            migrationBuilder.DropTable(
                name: "tee_times");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
