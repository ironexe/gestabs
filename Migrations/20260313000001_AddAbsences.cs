using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyAvaloniaApp.Migrations;

public partial class AddAbsences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "absences",
            columns: table => new
            {
                id           = table.Column<int>(nullable: false)
                                    .Annotation("Npgsql:ValueGenerationStrategy",
                                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ppr          = table.Column<int>(nullable: false),
                absence_date = table.Column<DateTime>(nullable: false),
                hours        = table.Column<decimal>(nullable: false),
                absence_type = table.Column<string>(nullable: false),
                notes        = table.Column<string>(nullable: true),
                created_at   = table.Column<DateTime>(nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_absences", x => x.id);
                table.ForeignKey(
                    name:            "FK_absences_personnel_ppr",
                    column:          x => x.ppr,
                    principalTable:  "personnel",
                    principalColumn: "ppr",
                    onDelete:        ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name:   "IX_absences_ppr",
            table:  "absences",
            column: "ppr");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "absences");
    }
}