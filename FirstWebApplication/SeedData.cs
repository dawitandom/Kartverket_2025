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

        // ===== Organisasjoner (NLA, Luftforsvaret, Kartverket) =====
        // Forutsetter at du har DbSet<Organization> Organizations og DbSet<OrganizationUser> OrganizationUsers i ApplicationContext
        if (db.Organizations != null && !await db.Organizations.AnyAsync())
        {
            db.Organizations.AddRange(
                new Organization { Name = "Norsk Luftambulanse", ShortCode = "NLA" },
                new Organization { Name = "Luftforsvaret", ShortCode = "LFS" },
                new Organization { Name = "Kartverket", ShortCode = "KRT" }
            );
            await db.SaveChangesAsync();
        }

        // ===== OrgAdmin-brukere knyttet til hver organisasjon =====
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
                // Create or update user
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

                // Link user to organization (find by ShortCode, fall back to first org)
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
