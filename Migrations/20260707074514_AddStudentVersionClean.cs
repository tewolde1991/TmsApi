using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentVersionClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                 migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Students",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Students");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Students",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
