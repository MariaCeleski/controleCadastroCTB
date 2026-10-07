using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccessControl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyRegistrationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "companies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Existing records receive a deterministic, organization-scoped sequential value before uniqueness is enforced.
            migrationBuilder.Sql("""
                WITH ranked_companies AS (
                    SELECT "Id", LPAD(ROW_NUMBER() OVER (PARTITION BY "OrganizationId" ORDER BY "CreatedAt", "Id")::text, 6, '0') AS "RegistrationNumber"
                    FROM companies
                )
                UPDATE companies AS company
                SET "RegistrationNumber" = ranked_companies."RegistrationNumber"
                FROM ranked_companies
                WHERE company."Id" = ranked_companies."Id";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "RegistrationNumber",
                table: "companies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_companies_OrganizationId_RegistrationNumber",
                table: "companies",
                columns: new[] { "OrganizationId", "RegistrationNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_companies_OrganizationId_RegistrationNumber",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "companies");
        }
    }
}
