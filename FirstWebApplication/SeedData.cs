
using Microsoft.AspNetCore.Identity;
using FirstWebApplication.Models;
using System.Threading.Tasks;

namespace FirstWebApplication;

public static class SeedData
{
    public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create four roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("Registrar"))
        {
            await roleManager.CreateAsync(new IdentityRole("Registrar"));
        }

        if (!await roleManager.RoleExistsAsync("Pilot"))
        {
            await roleManager.CreateAsync(new IdentityRole("Pilot"));
        }

        if (!await roleManager.RoleExistsAsync("Entrepreneur"))
        {
            await roleManager.CreateAsync(new IdentityRole("Entrepreneur"));
        }

        // Create Admin user (system administrator - can manage users)
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

            var result = await userManager.CreateAsync(adminUser, "admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Create Registrar user (approves/rejects reports)
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

            var result = await userManager.CreateAsync(registrarUser, "registrar123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(registrarUser, "Registrar");
            }
        }

        // Create Pilot user (submits reports)
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

            var result = await userManager.CreateAsync(pilotUser, "pilot123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(pilotUser, "Pilot");
            }
        }

        // Create Entrepreneur user (submits reports, same access as Pilot)
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

            var result = await userManager.CreateAsync(entrepreneurUser, "entrepreneur123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(entrepreneurUser, "Entrepreneur");
            }
        }
    }
}