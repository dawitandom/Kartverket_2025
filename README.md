# **DRIFT**

## **Sette opp applikasjon**
- Åpne Terminal
- Klon Repositoryet
  ```bash
  git clone <link>
- Åpne Docker Desktop og sjekk at den kjører (engine running skal stå)
- Naviger til følgende mappe i Terminal: Kartverket_2025
- Skriv inn følgende kommandoer:
  ```bash
  docker compose down -v
  docker compose build
  docker compose up -d
- Nå skal to container i docker ha startet med firstwebapplication og mariadbcontainer
- Åpne http://localhost:8000
- Åpne https://localhost:8001 (NB: Kommer advarsel. Trykk vis mer og besøk nettside. Kan se annerledes ut ifra hvilke nettleser som brukes)

- Dersom kommandoene ikke funket, prøv disse:
  ```bash
  docker compose down -v
  docker builder prune -af
  docker compose build --no-cache
  docker compose up -d


## **Kikk inn I databasen (via container)**

- Åpne Terminal
- Logg inn i database:
  ```bash
  docker exec -it mariadb mariadb -u appuser -p
- Passord:
  ```bash
  werHTG123
- Velg riktig database
  ```bash
  USE kartverket;
- Sjekk at tabellene finnes (Reports, ObstacleTypes, AspNetUsers, AspNetRoles og lignende)
  ```bash
  SHOW TABLES;


## **Kjøre tester**
- Åpne terminal
- Naviger til Kartverket_2025/FirstWebApplication.Tests
- Kjør testene
- ```bash
  dotnet test

# **Arcitecture**

**Kartverket_2025**
- docker-compose.yml
- docker-compose.dcproj
- FirstWebApplication.sln
- README.md
- launchSettings.json

  **FirstWebApplication**
  - FirstWebApplication.csproj
  - Dockerfile
  - appsettings.json
  - appsettings.Development.json
  - Program.cs
  - SeedData.cs

    **DataContext**
    - ApplicationContext.cs

    **Models**
    - ApplicationUser.cs
    - ErrorViewModel.cs
    - ObstacleTypeEntity.cs
    - Report.cs
    - Notifications.cs
    - Organization.cs
    - OrganizationUser.cs

      **ViewModel**
      - RegisterViewModel.cs
      - CreateUserViewModel.cs
      - UserManagementViewModel.cs
      - MyProfileViewModel.cs
      - OrgMembersViewModel.cs
      - CreateOrgAdminViewModel.cs

    **Repository**
    - IReportRepository.cs
    - ReportRepository.cs

    **Controllers**
    - AccountController.cs
    - ReportController.cs
    - HomeController.cs
    - AdminController.cs
    - NotificationController.cs
    - OrgAdminController.cs
    - OrganizationAdminController.cs
    - ProfileController.cs

    **Migrations**
    - 20251022095924_InitialCreateWithIdentity.cs
    - 20251022095924_InitialCreateWithIdentity.Designer.cs
    - 20251028105820_AddReportLastUpdated.cs
    - 20251028105820_AddReportLastUpdated.Designer.cs
    - 20251117094403_AddOrganizations.cs
    - 20251117094403_AddOrganizations.Designer.cs
    - 20251117103357_RenameAltitudeToHeight.cs
    - 20251117103357_RenameAltitudeToHeight.Designer.cs
    - 20251117134008_AddReportGeometryColumn.cs
    - 20251117134008_AddReportGeometryColumn.Designer.cs
    - 20251117151528_AddNotifications.cs
    - 20251117151528_AddNotifications.Designer.cs
    - 20251118090653_AddRegistrarCommentToReport.cs
    - 20251118090653_AddRegistrarCommentToReport.Designer.cs
    - 20251118210333_MakeReportDescriptionAndObstacleNullable.cs
    - 20251118210333_MakeReportDescriptionAndObstacleNullable.Designer.cs
    - ApplicationContextModelSnapshot.cs

    **Views**
    - _ViewImports.cshtml
    - _ViewStart.cshtml

      **Shared**
      - _Layout.cshtml
      - _Layout.cshtml.css
      - Error.cshtml
      - _ValidationScriptsPartial.cshtml

      **Account**
      - Login.cshtml
      - Register.cshtml

      **Admin**
      - CreateUser.cshtml
      - ManageUsers.cshtml

      **Home**
      - Index.cshtml
      - About.cshtml
      - Privacy.cshtml

      **Report**
      - Scheme.cshtml
      - Edit.cshtml
      - Details.cshtml
      - RegistrarDetails.cshtml
      - MyReports.cshtml
      - PendingReports.cshtml
      - ReviewedReports.cshtml
      - AllReports.cshtml

      **Notification**
      - Index.cshtml

      **OrgAdmin**
      - Members.cshtml
      - OrgReports.cshtml

      **OrganizationAdmin**
      - Create.cshtml
      - CreateOrgAdmin.cshtml
      - Index.cshtml

      **Profile**
      - Index.cshtml

    **Properties**
    - launchSettings.json

    **wwwroot**
    - favicon.ico

      **css**
      - site.css

      **lib**
        **leaflet**
        - leaflet.css
        - leaflet.js

  **FirstWebApplication.Tests**
  - FirstWebApplication.Tests.csproj

    **Tests**
    - AccountControllerUnitTests.cs
    - ReportControllerAuthUnitTests.cs
    - ReportValidationTests.cs

      **Fakes**
      - FakeSignInManager.cs


# **Testing**
For å sikre at applikasjonen fungerer som forventet, har vi gjennomført flere typer testing: enhetstesting, systemstesting, sikkerhetstesting og brukervennlighetstesting. Under følger en oversikt over hva som er testet og resultatene.

## **1. Enhetstesting (xUnit)**
Enhetstestene ligger i prosjektet **FirstWebApplication.Tests**.

### ReportValidationTests
- Tester validering av `Report`:
  - `Description` må være minst **10 tegn**
  - `HeightFeet` må være mellom **0–20 000**
- Feil verdier gir valideringsfeil som forventet.

### AccountControllerUnitTests
Tester login-flyten:
- Tomt brukernavn/passord → feilmelding  
- Feil passord → feilmelding  
- Riktig passord → redirect til *Home*

### ReportControllerAuthUnitTests
Tester tilgangskontroll i `ReportController`:
- Pilot kan kun redigere egne **Draft**-rapporter  
- Pilot kan **ikke** endre Approved/Rejected  
- Uautorisert tilgang gir korrekt redirect

### **Kjøre testene**
-```bash
  cd FirstWebApplication.Tests
  dotnet test

- Alle enhetstester passerte.

## **2. Systemstesting (ende-til-ende)**

| # | Scenario                                   | Steg                                                                                         | Forventet resultat                                                                                 | Resultat                |
|---|--------------------------------------------|----------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------|--------------------------|
| 1 | Opprette og sende inn rapport (Pilot)      | Login som `pilot` → New Report → fyll ut alle felter → klikk i kartet → Submit              | Redirect til *My Reports*; ny rad med status **Pending**                                           | Funka                   |
| 2 | Lagre rapport som kladd                    | Login som `pilot` → New Report → velg kun posisjon → Save as draft                          | Rapport vises i *My Reports* med status **Draft**                                                  | Funka                   |
| 3 | Validering ved for kort beskrivelse        | New Report → fyll inn posisjon + obstacle, men kort description (<10 tegn) → Submit         | Valideringsfeil på Description, rapport ikke lagret                                                | Feilet som forventet     |
| 4 | Registrar godkjenner rapport               | Login som `registrar` → Pending Reports → åpne rapport → Approved → Save                    | Status endres til **Approved**, forsvinner fra Pending                                             | Funka                   |
| 5 | Notification ved godkjenning               | Etter scenario 4: login som `pilot` → åpne Notifications                                     | Ulest varsel “Report … approved”; klikk åpner rapport                                              | Funka                   |
| 6 | OrgAdmin ser rapporter for egen org        | Login som OrgAdmin → Org Reports → filtrer på status Pending                                | Viser kun rapporter fra brukere i samme organisasjon                                               | Funka                   |
| 7 | OrgAdmin legger til medlem                 | OrgAdmin → Members → skriv inn brukernavn → Add                                             | Medlem vises i liste; kobling i OrganizationUsers opprettes                                        | Funka                   |
| 8 | Egen konto med rapporter kan ikke slettes  | Pilot med rapport → Profile → Delete account                                                 | Feilmelding: bruker kan ikke slettes fordi rapporter finnes                                        | Feilet som forventet     |
| 9 | Egen konto uten rapporter kan slettes      | Bruker uten rapporter → Profile → Delete account                                             | Konto slettes og logout gjennomføres                                                               | Funka                   |
|10 | Admin sletter bruker                       | Admin → Manage Users → Delete                                                                | Brukeren slettes og fjernes fra liste; koblinger slettes                                           | Funka                   |

## **3. Sikkerhetstesting**
Vi testet flere av sikkerhetsmekanismene manuelt:

**Uinnlogget tilgang:**
- Forsøk på å åpne f.eks. /Report/MyReports, /Admin/ManageUsers uten å være innlogget
- Resultat: Redirected til login

**Rollebasert tilgang:**
- Pilot prøvde å åpne /Admin og /OrganizationAdmin
- Resultat: Tilgang nektes (403 / redirect)

**URL-manipulasjon:**
- Pilot A prøvde å åpne /Report/Details/<idTilPilotB>
- Resultat: Redirect + feilmelding (som forventet)

**Passordpolicy:**
- Forsøk på å registrere for kort passord
- Resultat: Feilmelding
  
**Anti-forgery:**
Alle POST-requests bruker anti-forgery token (valideres i controller).

**Integritet ved sletting:**
- Pilot med rapporter prøvde å slette egen konto
- Resultat: Feilmelding (konto kan ikke slettes pga. eksisterende rapporter)

Alt fungerte som forventet

## **4. Brukervennlighetstesting**
Vi gjorde enkel manuell brukertesting (hallway-metoden)

**Testpersoner fikk følgende oppgaver:**
- Logge inn som pilot, opprette hinder via kartskjemaet og finne rapporten sin
- Logge inn som registrar og vurdere en innmelding
- Navigere i løsningen på mobil
  
**Forbedringer gjort basert på tilbakemelding:**
- Klarere tekst på knapper og statuser
- Bedre spacing og større knapper
- Mer konsistent fargebruk i tabeller og status-badges

