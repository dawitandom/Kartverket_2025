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

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews(o =>
{
    // Alle "unsafe" HTTP-metoder (POST/PUT/PATCH/DELETE) krever CSRF-token automatisk
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Repositories
builder.Services.AddScoped<IReportRepository, ReportRepository>();

// === Database (MariaDB 11.8) + retry ===
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

// === DataProtection: persistér nøkler ulikt for container vs lokalt ===
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

// === Identity ===
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

// === Auth-cookie (sikkerhet forbedret) ===
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "Kartverket.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Endra fra Lax til Strict for bedre sikkerhet
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always; // Krev HTTPS i produksjon
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// === Anti-forgery (sikkerhet forbedret) ===
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "Kartverket.AntiForgery";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Strict; // Endra fra Lax til Strict
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.None
        : CookieSecurePolicy.Always; // Krev HTTPS i produksjon
    o.HeaderName = "X-CSRF-TOKEN"; // For AJAX requests
});

// Språk/dato
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

// I prod: HTTPS/HSTS. I dev (lokalt og Docker): ikke tving HTTPS.
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

app.UseStaticFiles();
app.UseRouting();

// VIKTIG: Legg til antiforgery middleware (automatisk CSRF-beskyttelse)
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// === Migrer DB før seeding ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // NY SIGNATUR: sender inn db også
    await FirstWebApplication.SeedData.Initialize(userManager, roleManager, db);
}

app.Run();

// Gjør Program-klassen tilgjengelig for testing
public partial class Program { }
