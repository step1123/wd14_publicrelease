using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    public partial class AddYaicaServerNameYaicaSalted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "server_name",
                table: "server_role_ban",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "server_name",
                table: "server_ban",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "server_name",
                table: "server_role_ban");

            migrationBuilder.DropColumn(
                name: "server_name",
                table: "server_ban");
        }
    }
}
