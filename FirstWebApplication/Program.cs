using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FirstWebApplication;

/// <summary>
/// Hovedoppstartsfil for Kartverket Obstacle Reporting System.
/// Konfigurerer tjenester, database, autentisering og sikkerhet.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Konfigurerer MVC med automatisk CSRF-validering for alle usikre HTTP-metoder.
/// </summary>
builder.Services.AddControllersWithViews(o =>
{
    // Alle usikre HTTP-metoder (POST/PUT/PATCH/DELETE) krever CSRF-token automatisk
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

/// <summary>
/// Registrerer repositories for dependency injection.
/// </summary>
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

/// <summary>
/// Konfigurerer databaseforbindelse til MariaDB 11.8 med automatisk retry ved feil.
/// </summary>
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("OurDbConnection"),
        new MariaDbServerVersion(new Version(11, 8, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    )
);

/// <summary>
/// Konfigurerer Data Protection med forskjellig lagring av nøkler for container og lokalt miljø.
/// I Docker brukes /keys-volumet, lokalt brukes brukerens AppData-mappe.
/// </summary>
bool runningInContainer =
    string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase)
    || File.Exists("/.dockerenv");

var dp = builder.Services.AddDataProtection().SetApplicationName("Kartverket");

if (runningInContainer)
{
    // I Docker: bruk volumet /keys (mountet i docker-compose.yml)
    dp.PersistKeysToFileSystem(new DirectoryInfo("/keys"));
}
else
{
    // Lokalt: lagre i brukerens AppData (skrivbar på Windows/macOS/Linux)
    var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var localKeysPath = Path.Combine(basePath, "Kartverket", "keys");
    Directory.CreateDirectory(localKeysPath);
    dp.PersistKeysToFileSystem(new DirectoryInfo(localKeysPath));
}

/// <summary>
/// Konfigurerer ASP.NET Core Identity med sterke passordkrav og låsefunksjon.
/// Passord må ha minst 12 tegn, inneholde store/små bokstaver, tall og spesialtegn.
/// </summary>
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 12;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

/// <summary>
/// Konfigurerer autentiseringscookie med forbedret sikkerhet.
/// Bruker Strict SameSite og krever HTTPS i produksjon.
/// </summary>
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Kartverket.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Endret fra Lax til Strict for bedre sikkerhet
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always; // Krever HTTPS i produksjon
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

/// <summary>
/// Konfigurerer anti-forgery beskyttelse mot CSRF-angrep.
/// Bruker Strict SameSite og krever HTTPS i produksjon.
/// </summary>
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "Kartverket.AntiForgery";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always;
    o.HeaderName = "X-CSRF-TOKEN"; // For AJAX-forespørsler
});

/// <summary>
/// Konfigurerer språk og datoformat til engelsk (en-US).
/// </summary>
var enUS = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = enUS;
CultureInfo.DefaultThreadCurrentUICulture = enUS;

var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new List<CultureInfo> { enUS },
    SupportedUICultures = new List<CultureInfo> { enUS }
};

var app = builder.Build();

app.UseRequestLocalization(locOptions);

/// <summary>
/// Konfigurerer HTTPS og feilhåndtering basert på miljø.
/// I produksjon: HTTPS/HSTS påkrevd. I utvikling: tillat HTTP.
/// </summary>
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

/// <summary>
/// Legger til sikkerhetsheaders for alle HTTP-responser.
/// Inkluderer XSS-beskyttelse, frame-options og HSTS (kun i produksjon).
/// </summary>
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // HSTS (HTTP Strict Transport Security) legges kun til i produksjon for å unngå HTTPS-tvang i utvikling
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append(
            "Strict-Transport-Security",
            "max-age=31536000; includeSubDomains; preload");
    }

    await next();
});

app.UseStaticFiles();
app.UseRouting();

/// <summary>
/// Legger til antiforgery middleware for automatisk CSRF-beskyttelse.
/// </summary>
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

/// <summary>
/// Konfigurerer standard rute for MVC-kontrollere.
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/// <summary>
/// Kjører database-migreringer og seeding av initial data.
/// Oppretter roller og testbrukere hvis de ikke allerede finnes.
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Ny signatur: sender inn db også
    await FirstWebApplication.SeedData.Initialize(userManager, roleManager, db);
}

app.Run();

/// <summary>
/// Gjør Program-klassen tilgjengelig for testing ved å gjøre den delvis.
/// </summary>
public partial class Program { }
