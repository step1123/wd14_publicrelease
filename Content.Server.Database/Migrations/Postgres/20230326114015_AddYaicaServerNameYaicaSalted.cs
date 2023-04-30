using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    public partial class AddYaicaServerNameYaicaSalted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "server_name",
                table: "server_role_ban",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "server_name",
                table: "server_ban",
                type: "text",
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
