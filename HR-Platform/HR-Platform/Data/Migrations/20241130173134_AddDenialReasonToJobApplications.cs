using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR_Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDenialReasonToJobApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DenialReason",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DenialReason",
                table: "JobApplications");
        }
    }
}
