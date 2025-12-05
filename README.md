# Innholdsfortegnelse

- [DRIFT](#drift)
  - [Sette opp applikasjon](#sette-opp-applikasjon)
  - [Kikk inn i databasen (via container)](#kikk-inn-i-databasen-via-container)

- [Roller og TestBrukere](#roller-og-testbrukere)
  - [Roller](#roller)
  - [Autoritet](#autoritet)
  - [TestBrukere](#testbrukere)

- [Architecture](#architecture)

- [Testing](#testing)
  - [1. Enhetstesting (xUnit)](#1-enhetstesting-xunit)
  - [2. Systemstesting (ende-til-ende)](#2-systemstesting-ende-til-ende)
  - [3. Sikkerhetstesting](#3-sikkerhetstesting)
  - [4. Brukervennlighetstesting](#4-brukervennlighetstesting)

- [Sikkerhet](#sikkerhet)
  - [Bruk av ASP.NET Core Identity](#bruk-av-aspnet-core-identity)
  - [Beskyttelse mot SQL-Injection](#beskyttelse-mot-sql-injection)
  - [Beskyttelse mot XSS (Cross-Site Scripting)](#beskyttelse-mot-xss-cross-site-scripting)
  - [Beskyttelse mot CSRF (Cross-Site Request Forgery)](#beskyttelse-mot-csrf-cross-site-request-forgery)
  - [Rollebasert tilgangskontroll](#rollebasert-tilgangskontroll)
  - [Inputvalidering](#inputvalidering)
 
- [Bruk av KI](#bruk-av-KI)

- [Bildekilde](#bildekilde)


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

Når applikasjonen kjører lokalt bruker vi et **selvsignert utviklersertifikat** for HTTPS. Dette sertifikatet er ikke utstedt av en offentlig, betrodd sertifikatutsteder (CA), og derfor klarer ikke nettleseren å verifisere identiteten til localhost på samme måte som for vanlige nettsider.

Det gir en advarsel første gang man besøker https://localhost:8001. Dette er forventet i et utviklingsmiljø, og vi bruker likevel HTTPS lokalt for å kunne teste:
- sikker-cookie-innstillinger (Secure, HttpOnly, SameSite=Strict)
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

# **Roller og TestBrukere**

## **Roller**
- Pilot (Piloter)
- Entrepreneur (Utbyggere)
- DefaultUser (Sivil)
- Registrar (Registerfører)
- Admin (SystemAdmin)
- OrgAdmin (OrganisasjonAdmin)

#### Autoritet:

Pilot, Entrepreneur, DefaultUser:
- Legge inn rapport (Alle felt må utfylles)
- Lagre kladd (Kun posisjon må fylles ut, og pilot kan senere fylle ut informasjon og submitte etter oppdrag)
- Se egne innrapporteringer
- Redigere eller slette egne innrapporteringer så lenge den ikke er behandlet (pending)
- Får varslinger ved endring av status på rapport
- Se profilen sin
- Slette profilen (Kun hvis brukeren ikke har noen rapporter)
- Sivile kan lage bruker (Blir DefaultUser)

Registrar
- Se nye innrapporterte hindringer (pending reports)
- Se behandlede rapporter (reviewed reports)
- Sortere rapporter etter Dato, Bruker og Hindring
- Filtrere rapporter etter Organisasjon og Status
- Kan se rollen og organisasjonen til innsender av rapport
- Kan redigerer innsendte rapporter før og etter godkjenning (for å sikre at godkjent rapport stemmer 100% med koordinater, hindring og høyde)
- Se profilen sin
- Slette profilen

OrgAdmin
- Se medlemmer av organisasjonen
- Legge til eller fjerne brukere fra organisasjonen
- Se alle rapporter innrapportert av medlemmer
- Søke etter bruker i rapporter, samt filtere status
- Se profilen sin
- Slette profilen

Admin
- Se alle brukere
- Slette og lage nye brukere
- Lage brukere med alle type roller
- Se, lage og slette organisasjoner
- Lage organisasjonsadministrator (orgadmin) for organisasjoner
- Se alle rapporter (all reports)
- Se nye innrapporterte hindringer (pending reports)
- Se behandlede rapporter (reviewed reports)
- Sortere rapporter etter Dato, Bruker og Hindring
- Filtrere rapporter etter Organisasjon og Status
- Kan se rollen og organisasjonen til innsender av rapport
- Kan redigerer innsendte rapporter før og etter godkjenning (tilfelle registerfører trenger hjelp eller blir syk, så kan admin hjelpe. Eller hvis det bare er en liten feil som må fikses)
- Se profilen sin

Registerfører (registrar) kan se organisasjonen og eventuelt rollen til innsender, slik at de kan prioritere innrapporteringer fra piloter for de store organisasjonene. Likevell er applikasjonen laget slik at også entrepreneurer i eller uten organisasjoner kan legge inn hindringer når de setter de opp (feks. kran på byggeplass). Dersom en sivil turgåer ser en hindring han ikke tror eller vet er innrapportert kan den også rapporteres inn ved at de oppretter en bruker (defaultUser) og legger inn hindring. Organisasjoner må kontakte de som drifter applikasjonen for å få opprettet organisasjon og orgadmin bruker. Deretter kan de rapportere inn hindringer med høyere prioritering. Bruken av roller og organisasjoner sikrer at alle kan bruke applikasjonen, og at disse brukes til å autentisere hvem som har rapportert inn hva. Dette gjør at registerfører sikkert kan prioritere det som er viktigst. 

#### TestBrukere:

Pilot:
- brukernavn:   pilot
- passord:      TestBruker123!

Entrepreneur:
- brukernavn:   entrepreneur
- passord:      TestBruker123!

Registerfører:
- brukernavn:   registrar
- passord:      TestBruker123!

OrgAdmin:
- brukernavn:   orgadmin_nla
- passord:      TestBruker123!

Admin:
- brukernavn:   admin
- passord:      TestBruker123!



# **Architecture**

[Se arkitekturdiagram (PDF)](FirstWebApplication/wwwroot/image/ArkitektDiagram.pdf)




# **Testing**
For å sikre at applikasjonen fungerer som forventet, har vi gjennomført flere typer testing: enhetstesting, systemstesting, sikkerhetstesting og brukervennlighetstesting. Under følger en oversikt over hva som er testet og resultatene.

## **1. Enhetstesting (xUnit)**
Enhetstestene ligger i prosjektet **FirstWebApplication.Tests**. Totalt **37 tester**.

### ReportValidationTests (3 tester)
Tester validering av Report-modellen:
- Description må være minst **10 tegn**
- HeightFeet må være mellom **0–3000**
- Gyldig rapport passerer validering

### AccountControllerUnitTests (3 tester)
Tester login-flyten:
- Tomt brukernavn/passord → feilmelding  
- Feil passord → feilmelding  
- Riktig passord → redirect til *Home*

### ReportControllerAuthUnitTests (9 tester)
Tester tilgangskontroll i ReportController:
- Pilot kan kun redigere egne **Draft**-rapporter  
- Pilot kan **ikke** endre Approved/Rejected  
- Ikke-eier får ikke redigere andres rapporter
- Ikke-eier får ikke se andres rapportdetaljer
- Admin kan se alle rapporter
- Ikke-eier får ikke slette andres rapporter
- Eier kan ikke slette godkjente rapporter
- Admin kan slette alle rapporter

### RegistrarApproveRejectTests (5 tester)
Tester Registrar-funksjonalitet:
- Godkjenning endrer status til **Approved**
- Avvisning endrer status til **Rejected**
- Feilmelding ved ugyldig rapport-ID
- Varsel opprettes til rapporteier ved godkjenning
- Varsel opprettes til rapporteier ved avvisning

### NotificationTests (5 tester)
Tester varslingssystemet:
- Henter varsler for innlogget bruker
- Markerer varsel som lest ved åpning
- Omdirigerer til rapport ved åpning av koblet varsel
- Feilmelding ved varsel som ikke finnes
- Markerer alle varsler som lest

### OrganizationTests (5 tester)
Tester validering av Organization-modellen:
- Gyldig organisasjon passerer validering
- Tomt navn gir feil
- Tom kortkode gir feil
- For lang kortkode (>10 tegn) gir feil
- For langt navn (>100 tegn) gir feil

### UserValidationTests (7 tester)
Tester validering av brukerregistrering:
- Gyldig RegisterViewModel passerer
- Tomt brukernavn gir feil
- Ugyldig e-post gir feil
- Passord-mismatch gir feil
- Tomt passord gir feil
- Gyldig CreateUserViewModel passerer
- Tom rolle gir feil

### **Kjøre testene**
```bash
  cd FirstWebApplication.Tests
  dotnet test
```

- Alle 37 enhetstester passerte.

## **2. Systemstesting (ende-til-ende)**

| # | Scenario                                   | Steg                                                                                         | Forventet resultat                                                                                 | Resultat                |
|---|--------------------------------------------|----------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------|--------------------------|
| 1 | Opprette og sende inn rapport (Pilot)      | Login som pilot → New Report → fyll ut alle felter → klikk i kartet → Submit              | Redirect til *My Reports*; ny rad med status **Pending**                                           | Funka                   |
| 2 | Lagre rapport som kladd                    | Login som pilot → New Report → velg kun posisjon → Save as draft                          | Rapport vises i *My Reports* med status **Draft**                                                  | Funka                   |
| 3 | Validering ved for kort beskrivelse        | New Report → fyll inn posisjon + obstacle, men kort description (<10 tegn) → Submit         | Valideringsfeil på Description, rapport ikke lagret                                                | Feilet som forventet     |
| 4 | Registrar godkjenner rapport               | Login som registrar → Pending Reports → åpne rapport → Approved → Save                    | Status endres til **Approved**, forsvinner fra Pending                                             | Funka                   |
| 5 | Notification ved godkjenning               | Etter scenario 4: login som pilot → åpne Notifications                                     | Ulest varsel "Report … approved"; klikk åpner rapport                                              | Funka                   |
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
- Identity-tabellene (AspNetUsers, AspNetRoles, AspNetUserRoles, osv.) administreres automatisk via ApplicationContext som arver fra IdentityDbContext.
- Egendefinerte roller brukes for tilgangsstyring:
  - Admin
  - Registrar
  - Pilot
  - Entrepreneur
  - DefaultUser
  - OrgAdmin
- Standardbrukere og roller seedes ved oppstart i SeedData.cs.
- Alle sensitive operasjoner er beskyttet med [Authorize] + detaljerte rollekrav:
  - Kun **Admin** får tilgang til AdminController
  - **Registrar/Admin** får bruke vurderingssider
  - **Piloter** får bare redigere og se egne rapporter
    
Autentisering skjer via trygg Identity-cookie med følgende egenskaper:
-	HttpOnly = true
-	SameSite = Strict
-	SecurePolicy = Always (i produksjon)
-	Eget navn: Kartverket.Auth
  
Dette gir sterk beskyttelse mot cookie-tyveri og session-angrep.

**Beskyttelse mot brute-force angrep:**
Applikasjonen har innebygd kontolåsing (lockout) som aktiveres etter gjentatte feilforsøk:
- Maks 10 mislykkede innloggingsforsøk før kontoen låses
- Låsetid: 5 minutter
- Gjelder for alle brukere

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
- Strict-Transport-Security (HSTS) kun i produksjon

**4. Ingen dynamisk innsetting av scripts**
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
- HeightFeet: 0–3 000 ft
- Alle obligatoriske felter har [Required]-attributter

# **Bruk av KI**
Under utviklingen av applikasjonen har vi brukt KI-verktøy som ChatGPT (GPT-5.1) og Microsoft Copilot. Disse verktøyene ble brukt som støtte i feilsøking av kode, forståelse av rammeverk (MVC, Docker, Entity Framework), samt læring av teknologier vi ikke hadde brukt tidligere, som CSS, HTML og JavaScript.
All kode er gjennomgått, bearbeidet og tilpasset av gruppen, og alt innhold vi leverer er vårt eget selvstendige arbeid. KI-verktøy har kun vært brukt som hjelpemiddel, i tråd med UiAs retningslinjer for bruk av kunstig intelligens i oppgaveskriving.

# **Bildekilde**
Bakgrunnsbildet til hjemmesiden er hentet fra freepik.com
