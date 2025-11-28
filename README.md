# **DRIFT**

## **Sette opp applikasjon**
- Åpne Terminal
- Klon Repositoryet
  ```bash
  git clone <link>
  ```
- Åpne Docker Desktop og sjekk at den kjører (engine running skal stå)
- Naviger til følgende mappe i Terminal: Kartverket_2025
- Skriv inn følgende kommandoer:
  ```bash
  docker compose down -v
  docker compose build
  docker compose up -d
  ```
- Nå skal to container i docker ha startet med firstwebapplication og mariadbcontainer
- Åpne http://localhost:8000
- Åpne https://localhost:8001 (NB: Kommer advarsel)

**Hvorfor kommer sikkerhetsvarsel?**

Når applikasjonen kjører lokalt bruker vi et **selvsignert utviklersertifikat** for HTTPS. Dette sertifikatet er ikke utstedt av en offentlig, betrodd sertifikatutsteder (CA), og derfor klarer ikke nettleseren å   verifisere identiteten til `localhost` på samme måte som for vanlige nettsider.

Det gir en advarsel første gang man besøker `https://localhost:8001`. Dette er forventet i et utviklingsmiljø, og vi bruker likevel HTTPS lokalt for å kunne teste:
- sikker-cookie-innstillinger (`Secure`, `HttpOnly`, `SameSite=Strict`)
- pålogging over kryptert forbindelse
- sikkerhetsheadere som normalt kun er aktive over HTTPS
I produksjon vil applikasjonen bruke et gyldig sertifikat fra en betrodd CA, og denne advarselen vil ikke oppstå.


## **Kikk inn I databasen (via container)**

- Åpne Terminal
- Logg inn i database:
  ```bash
  docker exec -it mariadb mariadb -u appuser -p
  ```
- Passord:
  ```bash
  werHTG123
  ```
- Velg riktig database
  ```bash
  USE kartverket;
  ```
- Sjekk at tabellene finnes (Reports, ObstacleTypes, AspNetUsers, AspNetRoles og lignende)
  ```bash
  SHOW TABLES;
  ```


# **Architecture**

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
      - allreports.css
      - orgreports.css
      - scheme.css
      - details.css
      - pendingreports.css
      - site.css
      - edit.css	
      - registrardetails.css
      - myreports.css
      - reviewedreports.css
 
      **js**
      - confirm.dialogs.js
      - map.report.details.js
      - filter.popover.js
      - map.report.edit.js
      - map.registrar.details.js
      - obstacle.popover.js
      - map.report.create.js
      - site.js

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
```bash
  cd FirstWebApplication.Tests
  dotnet test
```

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



# **Sikkerhet**
Dette prosjektet bruker flere sikkerhetsmekanismer i ASP.NET Core for å beskytte applikasjonen, brukerne og dataene. Under følger en oversikt over hvordan autentisering, autorisasjon, CSRF-beskyttelse, XSS-forebygging, og beskyttelse mot SQL-injection er implementert i koden.

## **Bruk av ASP.NET Core Identity**
- Applikasjonen bruker ASP.NET Core Identity for innlogging, brukerstyring og roller.
- Identity-tabellene (AspNetUsers, AspNetRoles, AspNetUserRoles, osv.) administreres automatisk via `ApplicationContext : IdentityDbContext<ApplicationUser>`.
- Egendefinerte roller brukes for tilgangsstyring:
  - Admin
  - Registrar
  - Pilot
  - Entrepreneur
  - DefaultUser
  - OrgAdmin
- Standardbrukere og roller seedes ved oppstart i `SeedData.cs`.
- Alle sensitive operasjoner er beskyttet med `[Authorize]` + detaljerte rollekrav:
  - Kun **Admin** får tilgang til AdminController
  - **Registrar/Admin** får bruke vurderingssider
  - **Piloter** får bare redigere og se egne rapporter
    
Autentisering skjer via trygg Identity-cookie med følgende egenskaper:
-	HttpOnly = true
-	SameSite = Strict
-	SecurePolicy = Always (i produksjon)
-	Eget navn: Kartverket.Auth
  
Dette gir sterk beskyttelse mot cookie-tyveri og session-angrep.

## **Beskyttelse mot SQL-Injection**
Applikasjonen bruker kun Entity Framework Core og LINQ for databaseaksess.
- Ingen rå SQL-strenger brukes i koden.
- Alle spørringer går via _context.Reports, _context.Users, _context.OrganizationUsers osv.
- EF Core parameteriserer verdier automatisk.
Eksempel (fra ReportRepository):

```csharp
return _context.Reports
    .Include(r => r.User)
    .Include(r => r.ObstacleType)
    .ToList();
```

Dette beskytter alle databaseoperasjoner mot SQL-injection uten behov for manuell sanitærlogikk.

## **Beskyttelse mot XSS (Cross-Site Scripting)**
Applikasjonen beskytter mot XSS med flere lag:

**1. Razor HTML-encoding**
Alle visninger bruker Razor (@Model.X) som automatisk HTML-enkoder innhold.
Brukerdata som rapportbeskrivelse, registrarkommentar eller brukernavn blir aldri renderet som rå HTML.

**2. Ingen bruk av Html.Raw**
Dette er viktig, fordi det hindrer at potensielt farlig input blir aktivt JavaScript.

**3. Sikkerhetsheadere i Program.cs**
Applikasjonen legger til:
- X-XSS-Protection: 1; mode=block
- X-Content-Type-Options: nosniff
- X-Frame-Options: DENY
- Referrer-Policy: strict-origin-when-cross-origin
- En Content-Security-Policy (CSP) som begrenser scripts og styles

**4. CSP brukes kun i produksjon**
CSP-headeren er ikke aktiv i utviklingsmiljøet (Development) fordi:
- Utviklingsmiljøet kjører vanligvis HTTP, ikke HTTPS.
- Flere verktøy brukes som unsafe-inline, lokale filer og eksterne script-kilder som ville blitt blokkert av en streng CSP.
- Under utvikling må man kunne bruke dev-verktøy, midlertidig styling og test-scripts som ikke er whitelisted.
Dette er et bevisst valg for å gjøre utviklingen smidigere.
I produksjon (HTTPS) er CSP aktiv og fungerer som en ekstra forsvarslinje mot XSS-angrep.

**5. Ingen dynamisk innsetting av scripts**
Script- og CSS-ressurser ligger i wwwroot og lastes statisk.

## **Beskyttelse mot CSRF (Cross-Site Request Forgery)**
**1. Global CSRF-beskyttelse**
```csharp
// Program.cs
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
```
Dette betyr at alle POST/PUT/PATCH/DELETE-requester automatisk må ha gyldig Anti-Forgery token.

**2. Egen Anti-Forgery cookie**
I oppsettet for antiforgery:
- Cookie-navn: Kartverket.AntiForgery
- HttpOnly = true
- SameSite = Strict
- SecurePolicy = Always (prod)
- Støtte for AJAX-token via X-CSRF-TOKEN header

**3. [ValidateAntiForgeryToken] på alle POST-metoder**
Alle controller-actions som endrer data har eksplisitt:
```csharp
[ValidateAntiForgeryToken]
```
Dette gjelder bl.a.:
- Opprettelse/redigering av rapporter
- Endring av status (Approve/Reject)
- Sletting av brukere
- Innlogging, registrering og utlogging
- Oppretting av organisasjoner og OrgAdmins

**4. Razor Forms genererer tokens automatisk**
Alle <form method="post"> får automatisk hidden-feltet:
```csharp
<input name="__RequestVerificationToken" ...>
```
Resultat: En angriper kan ikke lage skjulte POST-requests mot applikasjonen.

## **Rollebasert tilgangskontroll**
Hele applikasjonen bygger på Role-Based Access Control (RBAC). Under er eksempeler på dette:
- Pilot kan kun se og endre egne Draft/Pending rapporter
- Registrar kan se Pending og gjøre vurderinger
- Admin kan slette rapporter og administrere brukere
- OrgAdmin kan se kun rapporter fra egen organisasjon
Dette håndheves i controllerne via:
```csharp
[Authorize(Roles = "Registrar,Admin")]
[Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
```
Samt ved server-side sjekk av eierforhold på rapporter.

## **Inputvalidering**
Applikasjonen bruker ModelState + DataAnnotations for å validere brukerinput.
Eksempler:
- Rapportbeskrivelse: minimum 10 tegn
- Passord: minimum 12 tegn (Identity policy)
- HeightFeet: 0–20 000 ft
- Alle obligatoriske felter har [Required]-attributter


