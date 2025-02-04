using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPCoded.Migrations
{
    /// <inheritdoc />
    public partial class LoanType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoanType",
                table: "LoanApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoanType",
                table: "LoanApplications");
        }
    }
}
