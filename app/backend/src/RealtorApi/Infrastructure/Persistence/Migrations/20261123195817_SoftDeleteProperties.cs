using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtorApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SoftDeleteProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "properties");
        }
    }
}
