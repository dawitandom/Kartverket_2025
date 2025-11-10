using System.Collections.Generic;
using System.Security.Claims;
using FirstWebApplication.Controllers;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures; // TempDataDictionary
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FirstWebApplication.Tests;

public class ReportControllerAuthUnitTests
{
    private static ControllerContext AsUser(string userId, params string[] roles)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }

    private static ApplicationContext DummyCtx()
    {
        var opts = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase("unit-no-db-use")
            .Options;

        var ctx = new ApplicationContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public void Edit_OwnerAndNotDraft_ShouldRedirect()
    {
        // Arrange
        var repo = new Mock<IReportRepository>();
        repo.Setup(r => r.GetReportById("R1"))
            .Returns(new Report { ReportId = "R1", UserId = "U1", Status = "Pending" });

        var ctrl = new ReportController(repo.Object, DummyCtx())
        {
            ControllerContext = AsUser("U1", "Pilot")
        };

        // IMPORTANT: Seed TempData to avoid NullReference when controller writes to it
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = ctrl.Edit("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyReports", result!.ActionName);
    }
}