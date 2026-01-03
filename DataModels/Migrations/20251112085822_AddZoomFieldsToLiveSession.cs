using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Throb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddZoomFieldsToLiveSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VConnectLink",
                table: "LiveSessions",
                newName: "ZoomMeetingId");

            migrationBuilder.RenameColumn(
                name: "DiscordLink",
                table: "LiveSessions",
                newName: "StartUrl");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "LiveSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "JoinUrl",
                table: "LiveSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "LiveSessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "LiveSessions");

            migrationBuilder.DropColumn(
                name: "JoinUrl",
                table: "LiveSessions");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "LiveSessions");

            migrationBuilder.RenameColumn(
                name: "ZoomMeetingId",
                table: "LiveSessions",
                newName: "VConnectLink");

            migrationBuilder.RenameColumn(
                name: "StartUrl",
                table: "LiveSessions",
                newName: "DiscordLink");
        }
    }
}
