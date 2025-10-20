**Docker COMPOSE FUNKER FOR NÅ KUN PÅ WINDOWS, IMENS DATABASE FUNKER PÅ BEGGE**

Her kommer oppskirft til å lage database og compose

Docker Compose:

Kjør **Docker Compose** filen i Visual Studio

Hvis applikasjonen ikke åpnes automatisk, åpne **localhost:64453** i nettleser

Lokalt:
**dotnet run** i Kartverket_2025/FirstWebApplication

Database må være bygget og kjørt for at det funker:

**Database:**

**Terminal Bash:**
docker pull mariadb:11.8

docker run --name mariadbcontainer -e MYSQL_ROOT_PASSWORD=Passord -p 3307:3306 -d docker.io/library/mariadb:11.8

**Docker Desktop Terminal i Container**
docker exec -it mariadbcontainer mariadb -u root -p

Passordet er: Passord¨

CREATE DATABASE IF NOT EXISTS mariadbcontainer
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

use mariadbcontainer

**Terminal Bash:**
Åpne følgende mappe: FirstWebApplikasjon

dotnet ef database update

Hvis ikke dotnet ef database update funker betyr det at EF-verktøyene ikke er installert

dotnet tool update --global dotnet-ef

Deretter kan du prøve dotnet ef database update

**Hvordan sjekke at det funker**
Gå tilbake til docker dekstop terminalen hvor du er logget inn og databasen og skrevet use mariadbdatabase

SHOW TABLES;

For å se om det funker, så skal Reports, Users, UserRoles og ObstacleTypes vises







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





