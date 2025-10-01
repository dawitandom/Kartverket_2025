using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstWebApplication.Migrations
{
    // Dette er en migrasjon.Den brukes til å oppretter eller endre tabeller i databasen
    public partial class InitialMigration : Migration
    {
        // Denne metoden kjøres når migrasjonen blir lagt til (lager tabell)
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // setter database til å bruke utf8mb4
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");
            
            // Lager en tabell som heter "Advices"
            migrationBuilder.CreateTable(
                name: "Advices",
                columns: table => new
                {
                    // Kolonnen Title, tekst som må fylles ut
                    AdviceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),

                    // Kolonnen Title, tekst som må fylles ut
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    // Kolonnen Description, tekst som må fylles ut
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    // Setter AdviceId som primærnøkkel
                    table.PrimaryKey("PK_Advices", x => x.AdviceId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        // Dene metoden kjøres hvis man ruller migrasjonen tilbake (sletter tabell)
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sletter tabellen "Advices"
            migrationBuilder.DropTable(
                name: "Advices");
        }
    }
}
