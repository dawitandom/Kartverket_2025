# 📚 Eksamensdokumentasjon - Kartverket Obstacle Reporting System

## Innholdsfortegnelse
1. [Prosjektoversikt](#1-prosjektoversikt)
2. [Arkitektur og Design Patterns](#2-arkitektur-og-design-patterns)
3. [Database og Entity Framework](#3-database-og-entity-framework)
4. [Modeller (Models)](#4-modeller-models)
5. [Repository Pattern](#5-repository-pattern)
6. [Controllers - Detaljert gjennomgang](#6-controllers---detaljert-gjennomgang)
7. [ASP.NET Core Identity - Autentisering](#7-aspnet-core-identity---autentisering)
8. [Rollebasert Autorisasjon (RBAC)](#8-rollebasert-autorisasjon-rbac)
9. [Sikkerhet i detalj](#9-sikkerhet-i-detalj)
10. [Views og Razor](#10-views-og-razor)
11. [Dependency Injection](#11-dependency-injection)
12. [Docker og Deployment](#12-docker-og-deployment)
13. [Testing](#13-testing)
14. [Viktige kodeeksempler](#14-viktige-kodeeksempler)
15. [Vanlige eksamenssspørsmål](#15-vanlige-eksamenssspørsmål)

---

## 1. Prosjektoversikt

### Hva er applikasjonen?
Et **hindring-rapporteringssystem for Kartverket** hvor brukere (piloter, utbyggere, sivile) kan rapportere inn luftfartshindringer som kraner, master, tårn etc. Registerførere vurderer rapportene og godkjenner/avviser dem.

### Teknologi-stack
| Komponent | Teknologi |
|-----------|-----------|
| Backend | ASP.NET Core 9.0 (MVC) |
| Frontend | Razor Views, Bootstrap 5, Leaflet.js (kart) |
| Database | MariaDB 11.8 |
| ORM | Entity Framework Core 9.0 (Pomelo MySQL Provider) |
| Autentisering | ASP.NET Core Identity |
| Containerisering | Docker + Docker Compose |
| Testing | xUnit |

### Prosjektstruktur
```
Kartverket_2025/
├── FirstWebApplication/           # Hovedprosjektet
│   ├── Controllers/               # MVC Controllers
│   ├── Models/                    # Domenemodeller + ViewModels
│   ├── Views/                     # Razor Views
│   ├── Repository/                # Repository Pattern
│   ├── DataContext/               # EF Core DbContext
│   ├── Migrations/                # Database migrasjoner
│   ├── wwwroot/                   # Statiske filer (CSS, JS, bilder)
│   ├── Program.cs                 # Applikasjonens inngangspunkt
│   └── SeedData.cs                # Seeder for testdata
├── FirstWebApplication.Tests/     # Enhetstester
└── docker-compose.yml             # Container-oppsett
```

---

## 2. Arkitektur og Design Patterns

### MVC (Model-View-Controller)
Applikasjonen bruker **MVC-arkitekturen**:

```
[Bruker] → [Controller] → [Model/Repository] → [Database]
                ↓
            [View] → [Bruker]
```

**Fordeler med MVC:**
- **Separation of Concerns** - Klar ansvarsfordeling
- **Testbarhet** - Kan teste controllers isolert
- **Vedlikeholdbarhet** - Endringer i ett lag påvirker ikke andre

### Repository Pattern
Brukes for å **abstrahere dataaksess** fra controllers:

```csharp
// Interface definerer kontrakten
public interface IReportRepository
{
    IEnumerable<Report> GetAllReports();
    Report? GetReportById(string id);
    Task<Report> AddAsync(Report report);
    void UpdateReport(Report report);
    void DeleteReport(string id);
    Task SaveChangesAsync();
}

// Implementasjon bruker EF Core
public class ReportRepository : IReportRepository
{
    private readonly ApplicationContext _context;
    
    public IEnumerable<Report> GetAllReports()
    {
        return _context.Reports
            .Include(r => r.User)
            .Include(r => r.ObstacleType)
            .ToList();
    }
}
```

**Fordeler:**
- **Løs kobling** - Controller er ikke avhengig av EF Core direkte
- **Testbarhet** - Kan mocke repository i tester
- **Sentralisert logikk** - All dataaksess på ett sted

### Dependency Injection (DI)
ASP.NET Core bruker **Constructor Injection**:

```csharp
// Registrering i Program.cs
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

// Injection i controller
public class ReportController : Controller
{
    private readonly IReportRepository _reportRepository;
    
    public ReportController(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;  // Injisert automatisk
    }
}
```

**Livssykluser:**
- `AddScoped` - Ny instans per HTTP-request (vanligst for repositories)
- `AddTransient` - Ny instans hver gang den injiseres
- `AddSingleton` - Én instans for hele applikasjonens levetid

---

## 3. Database og Entity Framework

### ApplicationContext
Arver fra `IdentityDbContext` for å få Identity-tabeller:

```csharp
public class ApplicationContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) 
        : base(options) { }

    // Custom DbSets
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ObstacleTypeEntity> ObstacleTypes => Set<ObstacleTypeEntity>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationUser> OrganizationUsers => Set<OrganizationUser>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);  // VIKTIG: Konfigurerer Identity-tabeller
        
        // Fluent API for Report
        b.Entity<Report>(e =>
        {
            e.HasKey(x => x.ReportId);
            e.Property(x => x.ReportId).HasColumnType("char(10)");
            
            // Foreign key til User med Restrict (hindrer kaskade-sletting)
            e.HasOne(x => x.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Composite primary key for many-to-many
        b.Entity<OrganizationUser>(e =>
        {
            e.HasKey(x => new { x.OrganizationId, x.UserId });
        });
        
        // Seed data
        b.Entity<ObstacleTypeEntity>().HasData(
            new { ObstacleId = "CRN", ObstacleName = "Crane", SortedOrder = 1 },
            new { ObstacleId = "MST", ObstacleName = "Mast", SortedOrder = 2 }
        );
    }
}
```

### Database-tabeller
| Tabell | Beskrivelse |
|--------|-------------|
| AspNetUsers | Brukere (Identity) |
| AspNetRoles | Roller (Identity) |
| AspNetUserRoles | Bruker-rolle-kobling (Identity) |
| Reports | Hindring-rapporter |
| ObstacleTypes | Typer hindringer (Crane, Mast, etc.) |
| Organizations | Organisasjoner (NLA, Luftforsvaret) |
| OrganizationUsers | Many-to-many: Bruker ↔ Organisasjon |
| Notifications | Varsler til brukere |

### Migrasjoner
```bash
# Lag ny migrasjon
dotnet ef migrations add NyMigrasjon

# Kjør migrasjoner
dotnet ef database update

# I Program.cs kjøres dette automatisk:
await db.Database.MigrateAsync();
```

### Connection String
```json
// appsettings.json (lokalt)
"ConnectionStrings": {
    "OurDbConnection": "Server=localhost;Port=3307;Database=kartverket;User=appuser;Password=werHTG123"
}

// docker-compose.yml (container)
"Server=mariadb;Port=3306;Database=kartverket;User=appuser;Password=werHTG123"
```

---

## 4. Modeller (Models)

### Report - Hovedmodellen
```csharp
public class Report
{
    [Key]
    [Column(TypeName = "char(10)")]
    public string ReportId { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;  // FK til AspNetUsers
    
    [Column(TypeName = "decimal(11,9)")]
    public decimal? Latitude { get; set; }
    
    [Column(TypeName = "decimal(12,9)")]
    public decimal? Longitude { get; set; }
    
    [Column(TypeName = "Text")]
    public string? Geometry { get; set; }  // JSON for linjer/polygoner
    
    [Range(0, 3000, ErrorMessage = "Height must be between 0 and 3000 feet.")]
    public short? HeightFeet { get; set; }
    
    [MaxLength(3)]
    public string? ObstacleId { get; set; }  // FK til ObstacleTypes
    
    [StringLength(5000, MinimumLength = 10)]
    public string? Description { get; set; }
    
    [MaxLength(1000)]
    public string? RegistrarComment { get; set; }
    
    public DateTime DateTime { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";  // Draft, Pending, Approved, Rejected
    
    // Navigation properties
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }
    
    [ForeignKey("ObstacleId")]
    public ObstacleTypeEntity? ObstacleType { get; set; }
}
```

### ApplicationUser - Utvidet Identity-bruker
```csharp
public class ApplicationUser : IdentityUser  // Arver fra Identity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    // Navigation: Alle rapporter brukeren har laget
    public ICollection<Report> Reports { get; set; } = new List<Report>();
    
    // Navigation: Organisasjoner brukeren tilhører
    public ICollection<OrganizationUser> Organizations { get; set; } = new List<OrganizationUser>();
}
```

### Data Annotations (Validering)
| Attributt | Funksjon |
|-----------|----------|
| `[Required]` | Feltet er påkrevd |
| `[StringLength(max, MinimumLength = min)]` | Lengde-validering |
| `[Range(min, max)]` | Numerisk område |
| `[EmailAddress]` | E-post-format |
| `[MaxLength(n)]` | Maks lengde |
| `[Column(TypeName = "...")]` | Database-kolonnetype |
| `[ForeignKey("...")]` | Angir foreign key |

### ViewModels
Brukes for views som trenger data fra flere modeller:

```csharp
public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 12)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```

---

## 5. Repository Pattern

### Interface (Kontrakt)
```csharp
public interface IReportRepository
{
    Task<List<Report>> GetAllAsync();
    Task<Report> AddAsync(Report report);
    Task<Report> UpdateAsync(Report report);
    
    // Sync metoder for kompatibilitet
    IEnumerable<Report> GetAllReports();
    Report? GetReportById(string id);
    void AddReport(Report report);
    void UpdateReport(Report report);
    void DeleteReport(string id);
    Task SaveChangesAsync();
}
```

### Implementasjon
```csharp
public class ReportRepository : IReportRepository
{
    private readonly ApplicationContext _context;

    public ReportRepository(ApplicationContext context)
    {
        _context = context;
    }

    public IEnumerable<Report> GetAllReports()
    {
        return _context.Reports
            .Include(r => r.User)        // Eager loading av User
            .Include(r => r.ObstacleType) // Eager loading av ObstacleType
            .ToList();
    }

    public Report? GetReportById(string id)
    {
        return _context.Reports
            .Include(r => r.User)
            .Include(r => r.ObstacleType)
            .FirstOrDefault(r => r.ReportId == id);
    }

    public async Task<Report> AddAsync(Report report)
    {
        if (string.IsNullOrWhiteSpace(report.ReportId))
            report.ReportId = await GenerateUniqueReportId();
        
        report.DateTime = DateTime.Now;
        
        if (string.IsNullOrWhiteSpace(report.Status))
            report.Status = "Pending";

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    private async Task<string> GenerateUniqueReportId()
    {
        string reportId;
        bool exists;
        do
        {
            var timestamp = DateTime.Now.ToString("yyMMddHHmm");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 4);
            reportId = $"R{timestamp.Substring(0, 8)}{guid}".Substring(0, 10);
            exists = await _context.Reports.AnyAsync(r => r.ReportId == reportId);
        } while (exists);
        return reportId;
    }
}
```

### Eager Loading vs Lazy Loading
```csharp
// Eager Loading (anbefalt) - Laster relaterte data med en gang
var reports = _context.Reports
    .Include(r => r.User)           // Laster User
    .Include(r => r.ObstacleType)   // Laster ObstacleType
    .ToList();

// Uten Include ville User og ObstacleType vært null
```

---

## 6. Controllers - Detaljert gjennomgang

### ReportController
Håndterer all rapport-logikk.

#### Attributter
```csharp
[Authorize]  // Krever innlogging for hele controlleren
public class ReportController : Controller
{
    [HttpGet]
    [Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]  // Kun disse rollene
    public IActionResult Scheme() { }
    
    [HttpPost]
    [ValidateAntiForgeryToken]  // CSRF-beskyttelse
    public async Task<IActionResult> Scheme(Report report, string submitAction) { }
}
```

#### Create (Scheme) - GET og POST
```csharp
// GET: Viser skjemaet
[HttpGet]
[Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
public IActionResult Scheme()
{
    // Hent hindring-typer for dropdown
    var obstacleTypes = _db.ObstacleTypes
        .OrderBy(o => o.SortedOrder)
        .Select(o => new SelectListItem
        {
            Value = o.ObstacleId,
            Text = o.ObstacleName   
        })
        .ToList();

    ViewBag.ObstacleTypes = obstacleTypes;
    return View();
}

// POST: Lagrer rapporten
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Scheme(Report report, string submitAction)
{
    // Fjern felter som settes i controller fra validering
    ModelState.Remove(nameof(Report.ReportId));
    ModelState.Remove(nameof(Report.UserId));
    ModelState.Remove(nameof(Report.DateTime));
    
    var isSave = submitAction == "save";    // Lagre som kladd
    var isSubmit = submitAction == "submit"; // Send inn
    
    // SAVE: Trenger bare posisjon
    if (isSave)
    {
        ModelState.Remove(nameof(Report.ObstacleId));
        ModelState.Remove(nameof(Report.Description));
    }
    
    // Valider posisjon
    if (report.Latitude is null || report.Longitude is null)
    {
        ModelState.AddModelError("", "Location is required.");
    }
    
    // SUBMIT: Alle felter påkrevd
    if (isSubmit)
    {
        if (string.IsNullOrWhiteSpace(report.ObstacleId))
            ModelState.AddModelError(nameof(Report.ObstacleId), "Obstacle type is required.");
            
        if (string.IsNullOrWhiteSpace(report.Description) || report.Description.Length < 10)
            ModelState.AddModelError(nameof(Report.Description), "Description must be at least 10 characters.");
    }
    
    if (!ModelState.IsValid)
    {
        ViewBag.ObstacleTypes = /* ... */;
        return View(report);  // Returner skjema med feilmeldinger
    }
    
    // Sett verdier
    report.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    report.DateTime = DateTime.Now;
    report.Status = isSave ? "Draft" : "Pending";
    
    await _reportRepository.AddAsync(report);
    
    TempData["SuccessMessage"] = isSave 
        ? "Draft saved." 
        : "Report submitted!";
    
    return RedirectToAction("MyReports");
}
```

#### Approve/Reject med Notification
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Registrar,Admin")]
public async Task<IActionResult> Approve(string id)
{
    var report = _reportRepository.GetReportById(id);
    if (report == null)
    {
        TempData["ErrorMessage"] = "Report not found.";
        return RedirectToAction("PendingReports");
    }

    report.Status = "Approved";
    _reportRepository.UpdateReport(report);

    // Opprett varsel til brukeren
    _db.Notifications.Add(new Notification
    {
        UserId = report.UserId,
        ReportId = report.ReportId,
        Title = "Report approved",
        Message = $"Your report {report.ReportId} was approved.",
        CreatedAt = DateTime.Now,
        IsRead = false
    });

    await _reportRepository.SaveChangesAsync();
    await _db.SaveChangesAsync();

    TempData["SuccessMessage"] = $"Report {id} approved.";
    return RedirectToAction("PendingReports");
}
```

#### Details med rollebasert view
```csharp
[HttpGet]
public IActionResult Details(string id)
{
    var report = _reportRepository.GetReportById(id);
    if (report == null)
    {
        TempData["ErrorMessage"] = "Report not found.";
        return RedirectToAction("MyReports");
    }

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Piloter kan bare se egne rapporter
    if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur") || User.IsInRole("DefaultUser"))
    {
        if (report.UserId != userId)
        {
            TempData["ErrorMessage"] = "You don't have permission.";
            return RedirectToAction("MyReports");
        }
    }

    // Registrar/Admin får egen view
    if (User.IsInRole("Registrar") || User.IsInRole("Admin"))
        return View("RegistrarDetails", report);

    return View(report);
}
```

### AccountController
Håndterer autentisering:

```csharp
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    // LOGIN
    [HttpPost]
    [AllowAnonymous]  // Tillater ikke-innloggede
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Username and password are required";
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(
            username,
            password,
            isPersistent: true,      // Husk innlogging
            lockoutOnFailure: false  // Ikke lås konto ved feil
        );

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        if (result.IsLockedOut)
        {
            ViewBag.Error = "Account locked. Try again later.";
            return View();
        }

        ViewBag.Error = "Invalid username or password";
        return View();
    }

    // REGISTER
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Sjekk om brukernavn finnes
        var existing = await _userManager.FindByNameAsync(model.UserName);
        if (existing != null)
        {
            ModelState.AddModelError(nameof(model.UserName), "Username taken.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View(model);
        }

        // Gi rollen "DefaultUser"
        await _userManager.AddToRoleAsync(user, "DefaultUser");
        
        // Logg inn automatisk
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    // LOGOUT
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
```

### AdminController
Kun for Admin-rollen:

```csharp
[Authorize(Roles = "Admin")]  // Hele controlleren krever Admin
public class AdminController : Controller
{
    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var users = _userManager.Users.ToList();
        var userViewModels = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new UserManagementViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            });
        }

        return View(userViewModels);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
            TempData["SuccessMessage"] = "User created!";
            return RedirectToAction("ManageUsers");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("ManageUsers");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
            TempData["SuccessMessage"] = "User deleted!";
        else
            TempData["ErrorMessage"] = "Delete failed.";

        return RedirectToAction("ManageUsers");
    }
}
```

---

## 7. ASP.NET Core Identity - Autentisering

### Hva er Identity?
Et ferdig system for:
- Brukerregistrering og innlogging
- Passord-hashing (PBKDF2 med 100k iterasjoner)
- Rollebasert autorisasjon
- Lockout ved mange feil
- Cookie-basert autentisering

### Konfigurasjon i Program.cs
```csharp
// Registrer Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Passordkrav
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;

    // Lockout-innstillinger
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationContext>()  // Bruk vår DbContext
.AddDefaultTokenProviders();

// Konfigurer auth-cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Kartverket.Auth";
    options.Cookie.HttpOnly = true;           // Ikke tilgjengelig via JavaScript
    options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF-beskyttelse
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;  // Krev HTTPS i prod
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;  // Forny ved aktivitet
});
```

### Identity-tabeller i databasen
| Tabell | Innhold |
|--------|---------|
| AspNetUsers | Brukere med hashed passord |
| AspNetRoles | Roller (Admin, Pilot, etc.) |
| AspNetUserRoles | Bruker-rolle-koblinger |
| AspNetUserClaims | Bruker-claims |
| AspNetRoleClaims | Rolle-claims |
| AspNetUserLogins | Eksterne logins (OAuth) |
| AspNetUserTokens | Tokens for 2FA etc. |

### Viktige metoder
```csharp
// UserManager<ApplicationUser>
await _userManager.CreateAsync(user, password);     // Opprett bruker
await _userManager.FindByNameAsync(username);       // Finn bruker
await _userManager.AddToRoleAsync(user, "Pilot");   // Gi rolle
await _userManager.GetRolesAsync(user);             // Hent roller
await _userManager.DeleteAsync(user);               // Slett bruker

// SignInManager<ApplicationUser>
await _signInManager.PasswordSignInAsync(user, pass, persistent, lockout);
await _signInManager.SignInAsync(user, persistent);
await _signInManager.SignOutAsync();
```

---

## 8. Rollebasert Autorisasjon (RBAC)

### Roller i systemet
| Rolle | Rettigheter |
|-------|-------------|
| **Pilot** | Opprette/se/redigere egne rapporter |
| **Entrepreneur** | Samme som Pilot |
| **DefaultUser** | Samme som Pilot (sivile) |
| **Registrar** | Vurdere rapporter, godkjenne/avvise |
| **Admin** | Alt + brukeradministrasjon |
| **OrgAdmin** | Administrere egen organisasjon |

### Authorize-attributtet
```csharp
// Krever innlogging
[Authorize]
public IActionResult MyReports() { }

// Krever spesifikke roller
[Authorize(Roles = "Pilot,Entrepreneur,DefaultUser")]
public IActionResult Scheme() { }

// Krever Admin ELLER Registrar
[Authorize(Roles = "Admin,Registrar")]
public IActionResult PendingReports() { }

// Tillater alle (overskriver controller-nivå [Authorize])
[AllowAnonymous]
public IActionResult Login() { }
```

### Sjekke roller i kode
```csharp
// I controller
if (User.IsInRole("Admin"))
{
    // Admin-spesifikk logikk
}

// Hente bruker-ID
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

// Sjekke eierskap
if (report.UserId != userId)
{
    return Forbid();  // 403 Forbidden
}
```

### Rollesjekk i Views (Razor)
```html
@if (User.IsInRole("Admin"))
{
    <a asp-action="ManageUsers">Manage Users</a>
}

@if (User.IsInRole("Pilot") || User.IsInRole("Entrepreneur"))
{
    <a asp-action="Scheme">New Report</a>
}
```

---

## 9. Sikkerhet i detalj

### 1. CSRF-beskyttelse (Cross-Site Request Forgery)

**Hva er CSRF?**
Angrep hvor en ondsinnet side får brukerens nettleser til å utføre uønskede handlinger på en side de er logget inn på.

**Beskyttelse i Program.cs:**
```csharp
// Global CSRF-beskyttelse for alle POST/PUT/DELETE
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Anti-forgery cookie
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "Kartverket.AntiForgery";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.HeaderName = "X-CSRF-TOKEN";  // For AJAX
});
```

**I Razor-forms:**
```html
<form method="post">
    @Html.AntiForgeryToken()  <!-- Genererer hidden input med token -->
    <button type="submit">Submit</button>
</form>

<!-- Alternativt med Tag Helpers (automatisk) -->
<form asp-action="Scheme" method="post">
    <!-- Token legges til automatisk -->
</form>
```

### 2. SQL Injection-beskyttelse

**Beskyttet av Entity Framework:**
```csharp
// TRYGT - EF parameteriserer automatisk
var report = _context.Reports
    .Where(r => r.UserId == userId)
    .FirstOrDefault();

// Aldri gjør dette:
// var query = $"SELECT * FROM Reports WHERE UserId = '{userId}'";  // FARLIG!
```

### 3. XSS-beskyttelse (Cross-Site Scripting)

**Razor HTML-encoding:**
```html
<!-- Automatisk HTML-encodet - trygt -->
<p>@Model.Description</p>

<!-- FARLIG - ikke gjør dette med brukerdata -->
@Html.Raw(Model.Description)
```

**Security Headers i Program.cs:**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append(
            "Strict-Transport-Security",
            "max-age=31536000; includeSubDomains; preload");
    }
    
    await next();
});
```

### 4. Passord-sikkerhet
```csharp
options.Password.RequiredLength = 12;           // Minst 12 tegn
options.Password.RequireDigit = true;           // Tall
options.Password.RequireLowercase = true;       // Små bokstaver
options.Password.RequireUppercase = true;       // Store bokstaver
options.Password.RequireNonAlphanumeric = true; // Spesialtegn
```

### 5. Cookie-sikkerhet
| Innstilling | Funksjon |
|-------------|----------|
| `HttpOnly = true` | Cookie ikke tilgjengelig via JavaScript (XSS-beskyttelse) |
| `SameSite = Strict` | Cookie sendes kun til samme domene (CSRF-beskyttelse) |
| `SecurePolicy = Always` | Cookie kun over HTTPS (i prod) |

---

## 10. Views og Razor

### Layout (_Layout.cshtml)
```html
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - Kartverket</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    @RenderSection("Styles", required: false)
</head>
<body>
    <nav>
        @if (User?.Identity?.IsAuthenticated == true)
        {
            <span>@User.Identity?.Name</span>
            
            @if (User.IsInRole("Pilot"))
            {
                <a asp-action="Scheme" asp-controller="Report">New Report</a>
            }
        }
    </nav>

    <main>
        @RenderBody()  <!-- Her kommer innholdet fra child views -->
    </main>

    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Tag Helpers
```html
<!-- Lenker -->
<a asp-controller="Report" asp-action="MyReports">My Reports</a>
<a asp-action="Details" asp-route-id="@report.ReportId">View</a>

<!-- Forms -->
<form asp-action="Scheme" method="post">
    <input asp-for="Description" />           <!-- Binder til Model.Description -->
    <span asp-validation-for="Description" /> <!-- Viser valideringsfeil -->
    <button type="submit">Submit</button>
</form>

<!-- Select/dropdown -->
<select asp-for="ObstacleId" asp-items="ViewBag.ObstacleTypes">
    <option value="">-- Select --</option>
</select>

<!-- Valideringssummary -->
<div asp-validation-summary="All"></div>
```

### ViewBag vs ViewData vs TempData
| Type | Scope | Bruk |
|------|-------|------|
| `ViewBag` | Én request | Dynamiske data til view |
| `ViewData` | Én request | Dictionary med data |
| `TempData` | Én redirect | Meldinger etter redirect |

```csharp
// Controller
ViewBag.ObstacleTypes = obstacleTypes;
TempData["SuccessMessage"] = "Report saved!";
return RedirectToAction("MyReports");

// View
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success">@TempData["SuccessMessage"]</div>
}
```

---

## 11. Dependency Injection

### Registrering i Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);

// MVC med global CSRF
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Repositories (Scoped = ny instans per request)
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();

// Database
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("OurDbConnection"),
        new MariaDbServerVersion(new Version(11, 8, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();
```

### Livssykluser
```csharp
// Scoped - Ny instans per HTTP request (anbefalt for DbContext, repositories)
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// Transient - Ny instans hver gang den injiseres
builder.Services.AddTransient<IEmailService, EmailService>();

// Singleton - Én instans for hele applikasjonen
builder.Services.AddSingleton<ICacheService, CacheService>();
```

### Constructor Injection
```csharp
public class ReportController : Controller
{
    private readonly IReportRepository _reportRepository;
    private readonly ApplicationContext _db;
    private readonly IOrganizationRepository _organizationRepository;

    // DI-containeren injiserer automatisk avhengighetene
    public ReportController(
        IReportRepository reportRepository,
        ApplicationContext db,
        IOrganizationRepository organizationRepository)
    {
        _reportRepository = reportRepository;
        _db = db;
        _organizationRepository = organizationRepository;
    }
}
```

---

## 12. Docker og Deployment

### docker-compose.yml forklart
```yaml
services:
  mariadb:
    image: mariadb:11.8                  # Database-image
    container_name: mariadb
    environment:
      MARIADB_ROOT_PASSWORD: Passord
      MARIADB_DATABASE: kartverket       # Opprett database automatisk
      MARIADB_USER: appuser
      MARIADB_PASSWORD: werHTG123
    ports:
      - "3307:3306"                       # Host:Container port
    volumes:
      - mysql_data:/var/lib/mysql         # Persist data
    healthcheck:                          # Vent til DB er klar
      test: ["CMD", "mariadb-admin", "ping", "-h", "localhost"]
      interval: 5s
      timeout: 3s
      retries: 20

  web:
    build:
      context: .
      dockerfile: FirstWebApplication/Dockerfile
    container_name: firstwebapplication
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080;https://+:8081
      ConnectionStrings__OurDbConnection: "Server=mariadb;Port=3306;..."
    depends_on:
      mariadb:
        condition: service_healthy        # Vent på healthcheck
    ports:
      - "8000:8080"                        # HTTP
      - "8001:8081"                        # HTTPS
    volumes:
      - data_protection_keys:/keys        # Persist encryption keys

volumes:
  mysql_data:                             # Named volume for database
  data_protection_keys:                   # Named volume for keys
```

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FirstWebApplication/FirstWebApplication.csproj", "FirstWebApplication/"]
RUN dotnet restore "FirstWebApplication/FirstWebApplication.csproj"
COPY . .
WORKDIR "/src/FirstWebApplication"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FirstWebApplication.dll"]
```

### Kjøre applikasjonen
```bash
# Start containere
docker compose up -d

# Se logger
docker compose logs -f web

# Stopp og fjern
docker compose down -v
```

---

## 13. Testing

### xUnit Enhetstester
```csharp
public class ReportValidationTests
{
    // Hjelpemetode for validering
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]  // Én testcase
    public void TooShortDescription_ShouldFail()
    {
        var report = new Report
        {
            Description = "short"  // < 10 tegn
        };
        
        var results = Validate(report);
        
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Report.Description)));
    }

    [Theory]  // Parametrisert test
    [InlineData(-1)]
    [InlineData(25000)]
    public void Height_OutOfRange_ShouldFail(int value)
    {
        var report = new Report
        {
            HeightFeet = (short)value
        };
        
        var results = Validate(report);
        
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Report.HeightFeet)));
    }

    [Fact]
    public void ValidReport_ShouldPass()
    {
        var report = new Report
        {
            Description = "Valid description with more than 10 characters",
            HeightFeet = 500
        };
        
        var results = Validate(report);
        
        Assert.Empty(results);  // Ingen valideringsfeil
    }
}
```

### Kjøre tester
```bash
cd FirstWebApplication.Tests
dotnet test
```

---

## 14. Viktige kodeeksempler

### Middleware-pipeline i Program.cs
```csharp
var app = builder.Build();

// 1. Exception handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    app.UseDeveloperExceptionPage();
}

// 2. Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

// 3. Statiske filer (wwwroot)
app.UseStaticFiles();

// 4. Routing
app.UseRouting();

// 5. Anti-forgery (CSRF)
app.UseAntiforgery();

// 6. Autentisering (hvem er du?)
app.UseAuthentication();

// 7. Autorisasjon (har du tilgang?)
app.UseAuthorization();

// 8. MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 9. Kjør migrasjoner og seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();
    
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedData.Initialize(userManager, roleManager, db);
}

app.Run();
```

### LINQ-spørringer
```csharp
// Filtrering
var pendingReports = _context.Reports
    .Where(r => r.Status == "Pending")
    .ToList();

// Sortering
var sortedReports = reports
    .OrderByDescending(r => r.DateTime)
    .ToList();

// Joining med Include (Eager Loading)
var reportsWithUser = _context.Reports
    .Include(r => r.User)
    .Include(r => r.ObstacleType)
    .ToList();

// Gruppering (brukerroller)
var rolesLookup = (from ur in _db.UserRoles
                   join r in _db.Roles on ur.RoleId equals r.Id
                   group r.Name by ur.UserId into g
                   select new { UserId = g.Key, Roles = g.ToList() })
                  .ToDictionary(x => x.UserId, x => x.Roles);
```

---

## 15. Vanlige eksamenssspørsmål

### Arkitektur
1. **Forklar MVC-arkitekturen og hvordan den brukes i prosjektet.**
   - Model: Domenedata og forretningslogikk (Report, ApplicationUser)
   - View: Presentasjon (Razor views)
   - Controller: Håndterer requests, kobler M og V

2. **Hva er Repository Pattern og hvorfor bruker vi det?**
   - Abstraherer dataaksess fra controllers
   - Gjør testing enklere (kan mocke)
   - Sentraliserer databaselogikk

3. **Forklar Dependency Injection og livssykluser.**
   - Constructor injection av avhengigheter
   - Scoped (per request), Transient (alltid ny), Singleton (én)

### Database
4. **Hva er Entity Framework Core og hvordan brukes det?**
   - ORM som mapper objekter til database
   - DbContext for database-tilgang
   - LINQ for spørringer

5. **Forklar migrasjoner i EF Core.**
   - Versjonskontroll for databaseskjema
   - `dotnet ef migrations add`, `dotnet ef database update`

6. **Hva er Eager Loading vs Lazy Loading?**
   - Eager: .Include() laster relaterte data med én gang
   - Lazy: Relaterte data lastes ved behov (kan gi N+1-problem)

### Sikkerhet
7. **Hvordan beskytter applikasjonen mot CSRF-angrep?**
   - Anti-forgery tokens i forms
   - AutoValidateAntiforgeryTokenAttribute globalt
   - SameSite=Strict på cookies

8. **Hvordan beskytter applikasjonen mot SQL Injection?**
   - Entity Framework parameteriserer alle verdier
   - Ingen rå SQL-strenger med brukerdata

9. **Hvordan beskytter applikasjonen mot XSS?**
   - Razor HTML-encoder automatisk
   - Ingen bruk av Html.Raw med brukerdata
   - Security headers (X-XSS-Protection, CSP)

10. **Forklar ASP.NET Core Identity.**
    - Ferdig system for autentisering
    - Passord-hashing, roller, claims
    - UserManager og SignInManager

### Autorisasjon
11. **Forklar rollebasert autorisasjon (RBAC).**
    - Roller definerer rettigheter
    - [Authorize(Roles = "Admin")] på controllers/actions
    - User.IsInRole() for programmatisk sjekk

12. **Hva er forskjellen på autentisering og autorisasjon?**
    - Autentisering: Verifisere hvem du er (login)
    - Autorisasjon: Sjekke hva du har tilgang til (roller)

### Testing
13. **Hva er enhetstesting og hvordan testes validering?**
    - Tester én enhet (metode/klasse) isolert
    - ValidationContext + Validator.TryValidateObject
    - [Fact] og [Theory] attributter

### Docker
14. **Forklar docker-compose.yml filen.**
    - Definerer tjenester (web, database)
    - depends_on og healthcheck for oppstartsrekkefølge
    - Volumes for persistent lagring
    - Nettverk for kommunikasjon

---

## Lykke til med eksamen! 🎓

**Tips:**
- Forstå flyten fra request til response
- Kunne forklare sikkerhetstiltakene
- Vite hvorfor vi bruker ulike patterns
- Kunne peke på relevante kodefiler

