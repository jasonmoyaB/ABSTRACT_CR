using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Abstract_CR.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToPassResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration intentionally left empty. The schema already exists in the target database.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Baseline migration intentionally left empty.
        }
    }
}
