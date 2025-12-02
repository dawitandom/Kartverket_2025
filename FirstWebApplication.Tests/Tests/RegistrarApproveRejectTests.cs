using System.Collections.Generic;
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

/// <summary>
/// Tester for Registrar og Admin sine godkjennings- og avvisningsfunksjoner.
/// Verifiserer at kun autoriserte brukere kan endre rapportstatus.
/// </summary>
public class RegistrarApproveRejectTests
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
            .UseInMemoryDatabase("registrar-test-" + System.Guid.NewGuid())
            .Options;
        var ctx = new ApplicationContext(opts);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task Approve_ValidReport_ChangesStatusToApproved()
    {
        // Arrange
        var report = new Report { ReportId = "R1", UserId = "U1", Status = "Pending" };
        
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(report);
        reportRepo.Setup(r => r.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.CreateForReportStatusChangeAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Notification { Id = 1, UserId = "U1", Title = "Test", Message = "Test" });

        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(reportRepo.Object, notificationRepo.Object, orgRepo.Object, DummyCtx())
        {
            ControllerContext = AsUser("RegistrarUser", "Registrar")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Approve("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PendingReports", result!.ActionName);
        Assert.Equal("Approved", report.Status);
        reportRepo.Verify(r => r.UpdateAsync(report), Times.Once);
    }

    [Fact]
    public async Task Reject_ValidReport_ChangesStatusToRejected()
    {
        // Arrange
        var report = new Report { ReportId = "R1", UserId = "U1", Status = "Pending" };
        
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(report);
        reportRepo.Setup(r => r.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.CreateForReportStatusChangeAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Notification { Id = 1, UserId = "U1", Title = "Test", Message = "Test" });

        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(reportRepo.Object, notificationRepo.Object, orgRepo.Object, DummyCtx())
        {
            ControllerContext = AsUser("RegistrarUser", "Registrar")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Reject("R1") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PendingReports", result!.ActionName);
        Assert.Equal("Rejected", report.Status);
        reportRepo.Verify(r => r.UpdateAsync(report), Times.Once);
    }

    [Fact]
    public async Task Approve_NonExistentReport_ReturnsError()
    {
        // Arrange
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("INVALID")).ReturnsAsync((Report?)null);

        var notificationRepo = new Mock<INotificationRepository>();
        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(reportRepo.Object, notificationRepo.Object, orgRepo.Object, DummyCtx())
        {
            ControllerContext = AsUser("RegistrarUser", "Registrar")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Approve("INVALID") as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("PendingReports", result!.ActionName);
        Assert.Equal("Report not found.", ctrl.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task Approve_CreatesNotificationForReportOwner()
    {
        // Arrange
        var report = new Report { ReportId = "R1", UserId = "OwnerUser", Status = "Pending" };
        
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(report);
        reportRepo.Setup(r => r.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.CreateForReportStatusChangeAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Notification { Id = 1, UserId = "OwnerUser", Title = "Test", Message = "Test" });

        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(reportRepo.Object, notificationRepo.Object, orgRepo.Object, DummyCtx())
        {
            ControllerContext = AsUser("RegistrarUser", "Registrar")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        await ctrl.Approve("R1");

        // Assert - Verify notification was created for the report owner
        notificationRepo.Verify(n => n.CreateForReportStatusChangeAsync(
            "OwnerUser",  // UserId of report owner
            "R1",         // ReportId
            "Report approved",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Reject_CreatesNotificationForReportOwner()
    {
        // Arrange
        var report = new Report { ReportId = "R1", UserId = "OwnerUser", Status = "Pending" };
        
        var reportRepo = new Mock<IReportRepository>();
        reportRepo.Setup(r => r.GetByIdAsync("R1")).ReturnsAsync(report);
        reportRepo.Setup(r => r.UpdateAsync(It.IsAny<Report>())).Returns(Task.CompletedTask);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.CreateForReportStatusChangeAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Notification { Id = 1, UserId = "OwnerUser", Title = "Test", Message = "Test" });

        var orgRepo = new Mock<IOrganizationRepository>();

        var ctrl = new ReportController(reportRepo.Object, notificationRepo.Object, orgRepo.Object, DummyCtx())
        {
            ControllerContext = AsUser("RegistrarUser", "Registrar")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        await ctrl.Reject("R1");

        // Assert - Verify notification was created for the report owner
        notificationRepo.Verify(n => n.CreateForReportStatusChangeAsync(
            "OwnerUser",
            "R1",
            "Report rejected",
            It.IsAny<string>()), Times.Once);
    }
}

