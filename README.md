# Kartverket_2025 – Aviation Obstacles (ASP.NET Core MVC)

**Drift**

## 1) Overview
Small ASP.NET Core MVC app for reporting helicopter obstacles. A report includes:
- position from a map (Leaflet + OpenStreetMap),
- altitude in feet,
- obstacle type,
- a short message.

**Current storage:** in-memory (data resets on app restart). 
**Next step:** switch to EF Core + MariaDB/MySQL.

---

## 2) How to run (local dev)
Prereqs: **.NET 9 SDK**

bash
- dotnet run --project FirstWebApplication

Compose
- kjør compose også skal den bygge automatisk

Local
- Kjør appliasjonen vanlig




**Arcitecture**

Controllers
- HomeController (Landing page)
- ReportController (New Report -Get/Post- + All Reports

Models
- Report (Username, UserId, Message, Latitude, Longtitude, AltitudeFeet, Type, CreatedArUtc
- ObstacleType (enum)

Views
- Views/Report/Scheme.cshtml (form + Leaflet map)
- Views/Report/List.cshtml (table with submitted reports)
- Views/Shared/_Layout.cshtml (navbar/offcanvas menu)


**Tests and Results**


| # | Scenario        | Steps                                            | Expected                                     | Result |
| - | --------------- | ------------------------------------------------ | -------------------------------------------- | ------ |
| 1 | Happy path      | New Report → fill all → click map → Submit       | Redirect to All Reports, row visible         | Worked |
| 2 | Missing field   | Leave “Altitude (feet)” empty                    | Error “Altitude (feet) is required”          | Worked |
| 3 | Type not chosen | Keep placeholder                                 | Error “Obstacle type is required”            | Worked |
| 4 | Use my location | Click button                                     | Map pans, Lat/Lng filled                     | Worked |
| 5 | Culture/decimal | Lat/Lng use dot `.`                              | Submit works                                 | Worked |
| 6 | Empty state     | Open All reports with no data                    | “No reports yet.”                            | Worked |
| 7 | In-memory reset | Restart app                                      | List empty again                             | Worked |
