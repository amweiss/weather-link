#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WeatherLink.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkspaceTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    AppId = table.Column<string>(nullable: true),
                    TeamId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceTokens", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkspaceTokens");
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
