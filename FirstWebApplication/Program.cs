
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using FirstWebApplication.Repository;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Registrer repositories som tjenester
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IAdviceRepository, AdviceRepository>();

// LØSNING 2: EnableRetryOnFailure legges til her
builder.Services.AddDbContext<ApplicationContext>(options => 
    options.UseMySql(builder.Configuration.GetConnectionString("OurDbConnection"), 
        new MySqlServerVersion(new Version(11, 5, 2)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ASP.NET Core Identity - dette erstatter den gamle cookie-autentiseringen
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Språk og datoformatering: engelsk (USA)
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// LØSNING 1: Retry-logikk når applikasjonen starter
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<ApplicationContext>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // Vent på at databasen skal bli klar
    var retries = 10;
    var delay = TimeSpan.FromSeconds(5);
    
    for (int i = 0; i < retries; i++)
    {
        try
        {
            logger.LogInformation("Attempting to connect to database... (attempt {Attempt}/{Total})", i + 1, retries);
            context.Database.Migrate(); // Kjør migrasjoner
            await FirstWebApplication.SeedData.Initialize(userManager, roleManager);
            logger.LogInformation("Database seeded successfully!");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database not ready yet. Waiting {Delay} seconds... (attempt {Attempt}/{Total})", delay.TotalSeconds, i + 1, retries);
            
            if (i == retries - 1)
            {
                logger.LogError(ex, "Failed to connect to database after {Retries} attempts", retries);
                throw;
            }
            
            await Task.Delay(delay);
        }
    }
}

app.Run();