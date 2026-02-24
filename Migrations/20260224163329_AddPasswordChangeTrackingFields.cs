using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGMPKlik.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordChangeTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangeAt",
                schema: "identity",
                table: "UserSecurities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordChangedBy",
                schema: "identity",
                table: "UserSecurities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequirePasswordChange",
                schema: "identity",
                table: "UserSecurities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPasswordChangeAt",
                schema: "identity",
                table: "UserSecurities");

            migrationBuilder.DropColumn(
                name: "PasswordChangedBy",
                schema: "identity",
                table: "UserSecurities");

            migrationBuilder.DropColumn(
                name: "RequirePasswordChange",
                schema: "identity",
                table: "UserSecurities");
        }
    }
}
