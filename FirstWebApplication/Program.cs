using FirstWebApplication.DataContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Globalization;

// ---------- Builder ----------
var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// ---------- EF Core + MariaDB ----------
var connStr = builder.Configuration.GetConnectionString("OurDbConnection");
// Eksplisitt serverversjon (unngår AutoDetect ved design-time)
var serverVersion = new MySqlServerVersion(new Version(11, 4, 0));

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseMySql(connStr, serverVersion, mySqlOptions =>
    {
        // Gjør DB-tilkobling mer robust ved oppstart
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

// Språk/kultur (valgfritt – slik du hadde)
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

// ---------- Auto-migrate med mild retry ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

    var tries = 0;
    while (true)
    {
        try
        {
            db.Database.Migrate();
            break; // success
        }
        catch (Exception)
        {
            tries++;
            if (tries >= 10) throw; // gir opp etter ~50 sek
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    // I prod: standard sikkerhet/redirect
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    // I dev (Docker Compose): vi kjører HTTP og evt. HTTPS slik compose/ASPNETCORE_URLS sier
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
