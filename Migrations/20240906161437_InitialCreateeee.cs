using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameReviewApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateeee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Games",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Games");
        }
    }
}
