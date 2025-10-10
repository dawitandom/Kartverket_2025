Kartverket_2025 – Aviation Obstacles (ASP.NET Core MVC)

Operation (drift):

1) Overview
Small ASP.NET Core MVC app for reporting helicopter obstacles. A report includes:
- Front Page
- New Report
  - username and id
  - position from a map (Leaflet + OpenStreetMap),
  - altitude in feet,
  - obstacle type,
  - a short message.
- All reports


# Kjøring av applikasjonen (lokalt og via Docker Compose)

Dette prosjektet kan kjøres både **lokalt** (fra IDE/terminal) og gjennom **Docker Compose**.  
Dette er nyttig for å sikre at løsningen fungerer likt på både macOS og Windows.

---

## Førstegangsoppsett (kun én gang)

### 1. Installer nødvendig programvare
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)  
- [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download)

### 2. Opprett og tillit HTTPS-sertifikat
Dette er nødvendig for å kjøre HTTPS lokalt og i containeren.

```bash
mkdir -p https
dotnet dev-certs https -ep ./https/aspnetapp.pfx -p Passord
dotnet dev-certs https --trust
```

Dette lager et utviklersertifikat `aspnetapp.pfx` i mappen `https/`.  
Dette mountes inn i containeren automatisk via `docker-compose.override.yml`.

### 3. Opprett Docker-nettverk (hvis det ikke finnes)
```bash
docker network create appnet || true
```

---

## Kjøre lokalt (uten Docker)

### macOS
```bash
cd FirstWebApplication
dotnet restore
dotnet ef database update
dotnet run
```

### Windows (PowerShell)
```powershell
cd FirstWebApplication
dotnet restore
dotnet ef database update
dotnet run
```

**Tilgjengelige URL-er lokalt:**  
- HTTP → [http://localhost:5226](http://localhost:5226) *(kan være en annen port)*  
- HTTPS → [https://localhost:7166](https://localhost:7166)

Hvis porten allerede er i bruk, kan du stoppe tidligere prosesser via:
- macOS: `lsof -i :<port>` og `kill <PID>`
- Windows: `netstat -ano | findstr :<port>` og `taskkill /PID <PID> /F`

---

## Kjøre via Docker Compose

### macOS
```bash
docker compose down --remove-orphans
docker compose build
docker compose up -d
```

### Windows (PowerShell)
```powershell
docker compose down --remove-orphans
docker compose build
docker compose up -d
```

Dette starter både **MariaDB** og **.NET Web API** i containere.  
MariaDB tar ofte noen sekunder å bli klar første gang.

---

## Tilgjengelige URL-er i Compose

- HTTP → [http://localhost:8000](http://localhost:8000)  
- HTTPS → [https://localhost:8001](https://localhost:8001) *(må skrives manuelt)*

**NB!** Docker Desktop antar alltid HTTP når du klikker på porter —  
derfor må du selv endre `http` til `https` i adressefeltet for 8001.

---

##  Førstegangsfeil og løsninger

| Feil / symptom                                         | Årsak                                           | Løsning                                                                 |
|--------------------------------------------------------|--------------------------------------------------|--------------------------------------------------------------------------|
| `Unable to connect to any of the specified MySQL hosts` | MariaDB ikke ferdig oppstartet                  | Vent 10–15 sekunder og prøv igjen. Compose håndterer retry.              |
| `Address already in use`                               | Port i bruk fra tidligere kjøring               | Stopp gamle prosesser med `docker compose down` og evt. `lsof`/`netstat`.|
| `Safari cannot open page` på 8001                      | Docker Desktop åpner feil protokoll             | Åpne `https://localhost:8001` manuelt.                                   |
| Sertifikatfeil i nettleser                             | Sertifikatet er ikke trusted                    | Kjør `dotnet dev-certs https --trust` igjen.                             |

---

## �Rydde opp / Resette alt
Hvis noe låser seg helt:

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml down --remove-orphans
docker volume rm kartverket_2025_mariadb_data || true
docker rmi firstwebapplication:latest || true
```

Deretter bygger du på nytt:

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml build --no-cache
docker compose up -d
```

---

## Oppsummert

- **Lokal kjøring:**  
  ```bash
  dotnet run
  ```
  (kjøres inne i `FirstWebApplication`)

- **Compose:**  
  ```bash
  docker compose up -d
  ```
  (kjøres i prosjektroten)

- **HTTP:** `http://localhost:8000`  
- **HTTPS:** `https://localhost:8001` *(skrives manuelt)*  
- Databasen starter i container og kobles automatisk via `ConnectionStrings`.

---

## 🧭 Tips (valgfritt)

- Du kan også starte prosjektet direkte i **Visual Studio / Rider** uten terminal.  
  Da brukes de lokale portene (5226/7166 som standard).  
- Docker Desktop viser porter i UI — men husk at HTTPS må skrives inn manuelt.





Arcitecture:

Controllers
- HomeController (Landing page)
- ReportController (New Report -Get/Post- + All Reports

Models
- Report (Username, UserId, Message, Latitude, Longtitude, AltitudeFeet, Type, CreatedArUtc
- ObstacleType (enum)

DataContext
- ApplicationContext (Context to database (has been a bit here and there with the database. We have not yet succeeded to make it work so we have chosen til deliver something that works and fullfills task requirements)

Views
- Views/Report/Scheme.cshtml (form + Leaflet map)
- Views/Report/List.cshtml (table with submitted reports)
- Views/Shared/_Layout.cshtml (navbar/offcanvas menu)


Tests and Results:


| # | Scenario        | Steps                                            | Expected                                     | Result |
| - | --------------- | ------------------------------------------------ | -------------------------------------------- | ------ |
| 1 | Happy path      | New Report → fill all → click map → Submit       | Redirect to All Reports, row visable         | Worked |
| 2 | Missing field   | Leave “Altitude (feet)” empty                    | Error “Altitude (feet) is required”          | Worked |
| 3 | Type not chosen | Keep placeholder                                 | Eror “Obstacle type is required”             | Worked |
| 4 | Use my location | Click button                                     | Map pans, Lat/Lng filled                     | Worked |
| 5 | Culture/decimal | Lat/Lng use dot `.`                              | Submit works                                 | Worked |
| 6 | Empty state     | Open All reports with no data                    | “No reports yet.”                            | Worked |
| 7 | In-memory reset | Restart app                                      | List empty again                             | Worked |





