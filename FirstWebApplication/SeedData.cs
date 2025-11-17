using System.Threading.Tasks;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication;

public static class SeedData
{
    public static async Task Initialize(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationContext db)
    {
        // ===== Roller =====
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

        // ===== Admin-bruker (systemadmin) =====
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

        // ===== Registrar =====
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

        // ===== Pilot =====
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

        // ===== Entrepreneur =====
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

        // ===== Organisasjoner (NLA, Luftforsvaret, PHT) =====
        // Forutsetter at du har DbSet<Organization> Organizations og DbSet<OrganizationUser> OrganizationUsers i ApplicationContext
        if (db.Organizations != null && !await db.Organizations.AnyAsync())
        {
            db.Organizations.AddRange(
                new Organization { Name = "Norsk Luftambulanse", ShortCode = "NLA" },
                new Organization { Name = "Luftforsvaret", ShortCode = "LFS" },
                new Organization { Name = "Politiets helikoptertjeneste", ShortCode = "PHT" }
            );
            await db.SaveChangesAsync();
        }

        // ===== OrgAdmin-bruker knyttet til NLA =====
        if (db.Organizations != null && db.OrganizationUsers != null)
        {
            var orgAdminUser = await userManager.FindByNameAsync("orgadmin");
            if (orgAdminUser == null)
            {
                orgAdminUser = new ApplicationUser
                {
                    UserName = "orgadmin",
                    Email = "orgadmin@example.com",
                    FirstName = "Org",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(orgAdminUser, "TestBruker123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(orgAdminUser, "OrgAdmin");
                }
            }
            else
            {
                // Sørg for at eksisterende bruker har riktig rolle
                if (!await userManager.IsInRoleAsync(orgAdminUser, "OrgAdmin"))
                {
                    await userManager.AddToRoleAsync(orgAdminUser, "OrgAdmin");
                }
            }

            // Finn NLA (eller første org hvis NLA ikke finnes)
            var nla = await db.Organizations
                .OrderBy(o => o.OrganizationId)
                .FirstOrDefaultAsync(o => o.ShortCode == "NLA") 
                ?? await db.Organizations.OrderBy(o => o.OrganizationId).FirstOrDefaultAsync();

            if (nla != null)
            {
                var linkExists = await db.OrganizationUsers.AnyAsync(ou =>
                    ou.OrganizationId == nla.OrganizationId &&
                    ou.UserId == orgAdminUser.Id);

                if (!linkExists)
                {
                    db.OrganizationUsers.Add(new OrganizationUser
                    {
                        OrganizationId = nla.OrganizationId,
                        UserId = orgAdminUser.Id
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
