using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstWebApplication.Controllers;
using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
            .UseInMemoryDatabase("unit-no-db-use-" + System.Guid.NewGuid())
            .Options;

        var ctx = new ApplicationContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task Edit_OwnerAndNotDraft_ShouldRedirect()
    {
        // Arrange
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Approved" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("U1", "Pilot")
        };

        // IMPORTANT: Seed TempData to avoid NullReference when controller writes to it
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Edit("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyReports", result!.ActionName);
    }

    [Fact]
    public async Task Edit_NonOwner_ShouldRedirect()
    {
        // Arrange - User U2 trying to edit U1's report
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Draft" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("U2", "Pilot") // Different user
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Edit("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyReports", result!.ActionName);
    }

    [Fact]
    public async Task Edit_OwnerWithDraftStatus_ShouldShowView()
    {
        // Arrange - Owner editing their own Draft report
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Draft" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctx = DummyCtx();
        // Add obstacle types for the view (only if not already present)
        if (!ctx.ObstacleTypes.Any(o => o.ObstacleId == "TST"))
        {
            ctx.ObstacleTypes.Add(new ObstacleTypeEntity { ObstacleId = "TST", ObstacleName = "Test", SortedOrder = 99 });
            ctx.SaveChanges();
        }

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            ctx)
        {
            ControllerContext = AsUser("U1", "Pilot")
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Edit("R1") as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Report>(result!.Model);
    }

    [Fact]
    public async Task Details_NonOwner_ShouldRedirect()
    {
        // Arrange - User U2 trying to view U1's report (as Pilot)
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Pending" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("U2", "Pilot")
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Details("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyReports", result!.ActionName);
    }

    [Fact]
    public async Task Details_Admin_CanViewAnyReport()
    {
        // Arrange - Admin viewing someone else's report
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Pending" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("AdminUser", "Admin")
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Details("R1") as ViewResult;

        // Assert - Admin should see RegistrarDetails view
        Assert.NotNull(result);
        Assert.Equal("RegistrarDetails", result!.ViewName);
    }

    [Fact]
    public async Task Delete_NonOwner_ShouldRedirect()
    {
        // Arrange - User U2 trying to delete U1's report
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Draft" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("U2", "Pilot")
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Delete("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyReports", result!.ActionName);
    }

    [Fact]
    public async Task Delete_OwnerApprovedReport_ShouldRedirect()
    {
        // Arrange - Owner trying to delete an Approved report (not allowed)
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Approved" });

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("U1", "Pilot")
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Delete("R1") as RedirectToActionResult;

        // Assert - Should redirect because Approved reports can't be deleted by owner
        Assert.NotNull(result);
        Assert.Equal("MyReports", result!.ActionName);
    }

    [Fact]
    public async Task Delete_Admin_CanDeleteAnyReport()
    {
        // Arrange - Admin deleting any report
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1"))
            .ReturnsAsync(new Report { ReportId = "R1", UserId = "U1", Status = "Approved" });
        reportRepo.Setup(r => r.DeleteAsync("R1")).Returns(Task.CompletedTask);

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(
            reportRepo.Object,
            notificationRepo.Object,
            orgRepo.Object,
            DummyCtx())
        {
            ControllerContext = AsUser("AdminUser", "Admin")
        };

        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Delete("R1") as RedirectResult;

        // Assert - Admin can delete, redirects to AllReports
        Assert.NotNull(result);
        Assert.Contains("AllReports", result!.Url);
        reportRepo.Verify(r => r.DeleteAsync("R1"), Times.Once);
    }
}
