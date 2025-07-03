using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESD_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddLastFiredAtToAlertDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "AlertDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFiredAt",
                table: "AlertDefinitions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFiredAt",
                table: "AlertDefinitions");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "AlertDefinitions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
