using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    UniqueIdentifier = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DocumentTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DocumentLocation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    DocumentCreationDate = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DocumentAuthor = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DocumentTags = table.Column<string[]>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.UniqueIdentifier);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
