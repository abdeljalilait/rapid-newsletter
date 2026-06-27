using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsletterPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailProviderApiSecret : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedApiSecret",
                table: "email_provider_accounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedApiSecret",
                table: "email_provider_accounts");
        }
    }
}
