**Sette opp applikasjon**
- Klon repositoryet
- Åpne prosjektet i visual studio, rider eller lignende
- Naviger til følgende mappe: Kartverket_2025
- Skriv inn følgende kommandoer:
  ```bash
  docker compose down -v
  docker compose build
  docker compose up -d
- Nå skal en container i docker ha startet med firstwebapplication og mariadbcontainer
- Åpne http://localhost:8000

- Dersom kommandoene ikke funket, prøv disse:
  ```bash
  docker compose down -v
  docker builder prune -af
  docker compose build --no-cache
  docker compose up -d


Kikk inn I database:

Fra host: # Mac/Linux (krever mysql/mariadb-klienten installert)

mysql -h 127.0.0.1 -P 3307 -u appuser -p

passord: werHTG123


USE kartverket;

SHOW TABLES;


Eller via container:

docker exec -it mariadb mariadb -u appuser -p

passord: werHTG123


USE kartverket;

SHOW TABLES;



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





