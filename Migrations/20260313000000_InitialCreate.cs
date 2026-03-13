using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyAvaloniaApp.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "personnel",
            columns: table => new
            {
                ppr              = table.Column<int>(nullable: false),
                cina             = table.Column<string>(nullable: true),
                cinn             = table.Column<int>(nullable: true),
                nom_latin        = table.Column<string>(nullable: true),
                prenom_latin     = table.Column<string>(nullable: true),
                nom_arabe        = table.Column<string>(nullable: true),
                prenom_arabe     = table.Column<string>(nullable: true),
                genre            = table.Column<string>(nullable: true),
                sit_fam          = table.Column<string>(nullable: true),
                jour_nais        = table.Column<int>(nullable: true),
                mois_nais        = table.Column<int>(nullable: true),
                an_nais          = table.Column<int>(nullable: true),
                lieu_nais        = table.Column<string>(nullable: true),
                adresse          = table.Column<string>(nullable: true),
                code_postal      = table.Column<int>(nullable: true),
                ville            = table.Column<string>(nullable: true),
                tel_portable     = table.Column<string>(nullable: true),
                adresse_elec     = table.Column<string>(nullable: true),
                cd_position      = table.Column<string>(nullable: true),
                cd_statut        = table.Column<string>(nullable: true),
                cd_grade         = table.Column<string>(nullable: true),
                cd_cadre         = table.Column<string>(nullable: true),
                cd_discip        = table.Column<string>(nullable: true),
                cd_nation        = table.Column<string>(nullable: true),
                cd_dips          = table.Column<string>(nullable: true),
                cd_dipp          = table.Column<string>(nullable: true),
                modavgra         = table.Column<string>(nullable: true),
                echelon          = table.Column<int>(nullable: true),
                date_rec         = table.Column<DateTime>(nullable: true),
                dt_titul         = table.Column<DateTime>(nullable: true),
                anc_adm          = table.Column<DateTime>(nullable: true),
                anc_grade        = table.Column<DateTime>(nullable: true),
                dt_echelon       = table.Column<DateTime>(nullable: true),
                anc_echelon      = table.Column<DateTime>(nullable: true),
                date_position    = table.Column<DateTime>(nullable: true),
                dt_grade         = table.Column<DateTime>(nullable: true),
                dt_cadre         = table.Column<DateTime>(nullable: true),
                dt_dipscol       = table.Column<DateTime>(nullable: true),
                dt_dipprof       = table.Column<DateTime>(nullable: true),
                dt_sitstat       = table.Column<DateTime>(nullable: true),
                date_sitfam      = table.Column<DateTime>(nullable: true),
                affilie          = table.Column<bool>(nullable: true),
                dateaffil        = table.Column<DateTime>(nullable: true),
                num_affil        = table.Column<int>(nullable: true),
                num_imma         = table.Column<int>(nullable: true),
                villeform        = table.Column<string>(nullable: true),
                affectation_princ = table.Column<bool>(nullable: true),
                cd_last_etab     = table.Column<string>(nullable: true),
                cd_last_fonc     = table.Column<string>(nullable: true),
                id_last_annee    = table.Column<string>(nullable: true),
                id_tech          = table.Column<int>(nullable: true),
                imported_at      = table.Column<DateTime>(nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_personnel", x => x.ppr);
            });

        migrationBuilder.CreateTable(
            name: "activites",
            columns: table => new
            {
                id              = table.Column<int>(nullable: false)
                                       .Annotation("Npgsql:ValueGenerationStrategy",
                                                   NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ppr             = table.Column<int>(nullable: true),
                cina            = table.Column<string>(nullable: true),
                cd_modaffe      = table.Column<string>(nullable: true),
                cd_fonc         = table.Column<string>(nullable: true),
                cd_etab         = table.Column<string>(nullable: true),
                cd_service      = table.Column<int>(nullable: true),
                cd_division     = table.Column<int>(nullable: true),
                cd_entadm       = table.Column<int>(nullable: true),
                cd_cycle        = table.Column<string>(nullable: true),
                activ_princ     = table.Column<bool>(nullable: true),
                dt_deb_exercice = table.Column<DateTime>(nullable: true),
                dt_fin_exercice = table.Column<DateTime>(nullable: true),
                dateaffect      = table.Column<DateTime>(nullable: true),
                dt_aff_etab     = table.Column<DateTime>(nullable: true),
                dt_aff_prov     = table.Column<DateTime>(nullable: true),
                dt_aff_reg      = table.Column<DateTime>(nullable: true),
                dt_aff_poste    = table.Column<DateTime>(nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_activites", x => x.id);
                table.ForeignKey(
                    name:       "FK_activites_personnel_ppr",
                    column:     x => x.ppr,
                    principalTable: "personnel",
                    principalColumn: "ppr",
                    onDelete:   ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name:    "IX_activites_ppr",
            table:   "activites",
            column:  "ppr");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "activites");
        migrationBuilder.DropTable(name: "personnel");
    }
}
