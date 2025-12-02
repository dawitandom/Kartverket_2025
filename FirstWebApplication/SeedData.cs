using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication;

/// <summary>
/// Statisk klasse for seeding av initial data til databasen.
/// Oppretter roller, testbrukere og organisasjoner ved første oppstart.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Initialiserer databasen med roller, testbrukere og organisasjoner.
    /// Kjører kun hvis data ikke allerede finnes.
    /// </summary>
    /// <param name="userManager">UserManager for å opprette og administrere brukere</param>
    /// <param name="roleManager">RoleManager for å opprette og administrere roller</param>
    /// <param name="db">Databasekontekst for å lagre organisasjoner og koblinger</param>
    public static async Task Initialize(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationContext db)
    {
        // Oppretter alle systemroller hvis de ikke allerede finnes
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        if (!await roleManager.RoleExistsAsync("Registrar"))
            await roleManager.CreateAsync(new IdentityRole("Registrar"));

        if (!await roleManager.RoleExistsAsync("Pilot"))
            await roleManager.CreateAsync(new IdentityRole("Pilot"));

        if (!await roleManager.RoleExistsAsync("Entrepreneur"))
            await roleManager.CreateAsync(new IdentityRole("Entrepreneur"));

        if (!await roleManager.RoleExistsAsync("DefaultUser"))
            await roleManager.CreateAsync(new IdentityRole("DefaultUser"));

        if (!await roleManager.RoleExistsAsync("OrgAdmin"))
            await roleManager.CreateAsync(new IdentityRole("OrgAdmin"));

        // Oppretter admin-bruker (systemadministrator)
        if (await userManager.FindByNameAsync("admin") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@kartverket.no",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "TestBruker123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Oppretter registrar-bruker
        if (await userManager.FindByNameAsync("registrar") == null)
        {
            var registrarUser = new ApplicationUser
            {
                UserName = "registrar",
                Email = "registrar@kartverket.no",
                FirstName = "Registrar",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(registrarUser, "TestBruker123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(registrarUser, "Registrar");
            }
        }

        // Oppretter pilot-bruker
        if (await userManager.FindByNameAsync("pilot") == null)
        {
            var pilotUser = new ApplicationUser
            {
                UserName = "pilot",
                Email = "pilot@example.com",
                FirstName = "Test",
                LastName = "Pilot",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(pilotUser, "TestBruker123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(pilotUser, "Pilot");
            }
        }

        // Oppretter entrepreneur-bruker
        if (await userManager.FindByNameAsync("entrepreneur") == null)
        {
            var entrepreneurUser = new ApplicationUser
            {
                UserName = "entrepreneur",
                Email = "entrepreneur@example.com",
                FirstName = "Test",
                LastName = "Entrepreneur",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(entrepreneurUser, "TestBruker123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(entrepreneurUser, "Entrepreneur");
            }
        }

        // Oppretter standardorganisasjoner (NLA, Luftforsvaret, Kartverket)
        if (db.Organizations != null && !await db.Organizations.AnyAsync())
        {
            db.Organizations.AddRange(
                new Organization { Name = "Norsk Luftambulanse", ShortCode = "NLA" },
                new Organization { Name = "Luftforsvaret", ShortCode = "LFS" },
                new Organization { Name = "Kartverket", ShortCode = "KRT" }
            );
            await db.SaveChangesAsync();
        }

        // Oppretter OrgAdmin-brukere knyttet til hver organisasjon
        if (db.Organizations != null && db.OrganizationUsers != null)
        {
            var orgAdminSpecs = new[]
            {
                new { ShortCode = "NLA", UserName = "orgadmin_nla", Email = "orgadmin_nla@example.com", First = "NLA", Last = "Admin" },
                new { ShortCode = "LFS", UserName = "orgadmin_lfs", Email = "orgadmin_lfs@example.com", First = "LFS", Last = "Admin" },
                new { ShortCode = "KRT", UserName = "orgadmin_krt", Email = "orgadmin_krt@example.com", First = "KRT", Last = "Admin" }
            };

            foreach (var spec in orgAdminSpecs)
            {
                // Oppretter eller oppdaterer bruker
                var orgUser = await userManager.FindByNameAsync(spec.UserName);
                if (orgUser == null)
                {
                    orgUser = new ApplicationUser
                    {
                        UserName = spec.UserName,
                        Email = spec.Email,
                        FirstName = spec.First,
                        LastName = spec.Last,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(orgUser, "TestBruker123!");
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(orgUser, "OrgAdmin");
                    }
                }
                else
                {
                    if (!await userManager.IsInRoleAsync(orgUser, "OrgAdmin"))
                    {
                        await userManager.AddToRoleAsync(orgUser, "OrgAdmin");
                    }
                }

                // Knytter bruker til organisasjon (finner via ShortCode, faller tilbake til første org hvis ikke funnet)
                var org = await db.Organizations.FirstOrDefaultAsync(o => o.ShortCode == spec.ShortCode)
                          ?? await db.Organizations.OrderBy(o => o.OrganizationId).FirstOrDefaultAsync();

                if (org != null)
                {
                    var linkExists = await db.OrganizationUsers.AnyAsync(ou =>
                        ou.OrganizationId == org.OrganizationId &&
                        ou.UserId == orgUser.Id);

                    if (!linkExists)
                    {
                        db.OrganizationUsers.Add(new OrganizationUser
                        {
                            OrganizationId = org.OrganizationId,
                            UserId = orgUser.Id
                        });
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
