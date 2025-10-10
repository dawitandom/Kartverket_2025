using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using FirstWebApplication.Repository;
using FirstWebApplication.DataContext;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAdviceRepository, AdviceRepository>(); //Legger til repository som en tjeneste

builder.Services.AddDbContext<ApplicationContext>(options => 
options.UseMySql(builder.Configuration.GetConnectionString("OurDbConnection"), 
new MySqlServerVersion(new Version(11, 5, 2))));

// Språk og datoformatering: engelsk (USA)
var enUS = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = enUS;
CultureInfo.DefaultThreadCurrentUICulture = enUS;

// Språkoppsett - vi bruker engelsk (USA) som standard
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new List<CultureInfo> { enUS },
    SupportedUICultures = new List<CultureInfo> { enUS }
};

var app = builder.Build();

app.UseRequestLocalization(locOptions); //Bruk språkoppsettet

// I produksjon skal vi vise en feilmeldingsside og bruke HSTS (for sikkerhet)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); //Bruk sikker tilkobling (https)

app.UseStaticFiles(); // Del ut filer fra wwwroot (med CSS, JS, bilder)

app.UseRouting(); // Finn riktig side når noen går til en adresse

app.UseAuthorization(); // Sjekk tilgang

// Standardadresse er /Home/Index (id er valgfri)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();