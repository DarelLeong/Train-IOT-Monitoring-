using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESD_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainIdToLoadWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainId",
                table: "LoadWeights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "LoadWeights");
        }
    }
}
