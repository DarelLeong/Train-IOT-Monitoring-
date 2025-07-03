using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESD_Project.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAlertRecipients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertDefinitionAlertRecipient");

            migrationBuilder.DropTable(
                name: "AlertRecipients");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "AlertDefinitions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AlertDefinitions");

            migrationBuilder.CreateTable(
                name: "AlertRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRecipients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertDefinitionAlertRecipient",
                columns: table => new
                {
                    AlertDefinitionsId = table.Column<int>(type: "int", nullable: false),
                    RecipientsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertDefinitionAlertRecipient", x => new { x.AlertDefinitionsId, x.RecipientsId });
                    table.ForeignKey(
                        name: "FK_AlertDefinitionAlertRecipient_AlertDefinitions_AlertDefinitionsId",
                        column: x => x.AlertDefinitionsId,
                        principalTable: "AlertDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertDefinitionAlertRecipient_AlertRecipients_RecipientsId",
                        column: x => x.RecipientsId,
                        principalTable: "AlertRecipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertDefinitionAlertRecipient_RecipientsId",
                table: "AlertDefinitionAlertRecipient",
                column: "RecipientsId");
        }
    }
}
