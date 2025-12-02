using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace FirstWebApplication.Tests;

/// <summary>
/// Tester for varslingsfunksjonalitet.
/// Verifiserer at varsler vises korrekt og kan markeres som lest.
/// </summary>
public class NotificationTests
{
    private static Mock<UserManager<ApplicationUser>> BuildUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static ControllerContext AsUser(string userId)
    {
        var claims = new List<Claim> 
        { 
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Pilot")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
    }

    [Fact]
    public async Task Index_ReturnsNotificationsForCurrentUser()
    {
        // Arrange
        var user = new ApplicationUser { Id = "U1", UserName = "pilot" };
        var notifications = new List<Notification>
        {
            new Notification { Id = 1, UserId = "U1", Title = "Test1", Message = "Message1", IsRead = false },
            new Notification { Id = 2, UserId = "U1", Title = "Test2", Message = "Message2", IsRead = true }
        };

        var userManager = BuildUserManager();
        userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.GetByUserIdAsync("U1")).ReturnsAsync(notifications);
        notificationRepo.Setup(n => n.GetUnreadCountAsync("U1")).ReturnsAsync(1);

        var ctrl = new NotificationController(notificationRepo.Object, userManager.Object)
        {
            ControllerContext = AsUser("U1")
        };

        // Act
        var result = await ctrl.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        var model = result!.Model as List<Notification>;
        Assert.NotNull(model);
        Assert.Equal(2, model!.Count);
        Assert.Equal(1, ctrl.ViewBag.UnreadCount);
    }

    [Fact]
    public async Task Open_MarksNotificationAsRead()
    {
        // Arrange
        var user = new ApplicationUser { Id = "U1", UserName = "pilot" };
        var notification = new Notification 
        { 
            Id = 1, 
            UserId = "U1", 
            Title = "Test", 
            Message = "Test message",
            IsRead = false,
            ReportId = null  // No linked report
        };

        var userManager = BuildUserManager();
        userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.GetByIdForUserAsync(1, "U1")).ReturnsAsync(notification);
        notificationRepo.Setup(n => n.MarkAsReadAsync(1)).Returns(Task.CompletedTask);

        var ctrl = new NotificationController(notificationRepo.Object, userManager.Object)
        {
            ControllerContext = AsUser("U1")
        };

        // Act
        var result = await ctrl.Open(1) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result!.ActionName);
        notificationRepo.Verify(n => n.MarkAsReadAsync(1), Times.Once);
    }

    [Fact]
    public async Task Open_WithLinkedReport_RedirectsToReportDetails()
    {
        // Arrange
        var user = new ApplicationUser { Id = "U1", UserName = "pilot" };
        var notification = new Notification 
        { 
            Id = 1, 
            UserId = "U1", 
            Title = "Report approved", 
            Message = "Your report was approved",
            IsRead = false,
            ReportId = "R123"  // Linked to report
        };

        var userManager = BuildUserManager();
        userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.GetByIdForUserAsync(1, "U1")).ReturnsAsync(notification);
        notificationRepo.Setup(n => n.MarkAsReadAsync(1)).Returns(Task.CompletedTask);

        var ctrl = new NotificationController(notificationRepo.Object, userManager.Object)
        {
            ControllerContext = AsUser("U1")
        };

        // Act
        var result = await ctrl.Open(1) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Details", result!.ActionName);
        Assert.Equal("Report", result.ControllerName);
        Assert.Equal("R123", result.RouteValues!["id"]);
    }

    [Fact]
    public async Task Open_NotificationNotFound_ReturnsError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "U1", UserName = "pilot" };

        var userManager = BuildUserManager();
        userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.GetByIdForUserAsync(999, "U1")).ReturnsAsync((Notification?)null);

        var ctrl = new NotificationController(notificationRepo.Object, userManager.Object)
        {
            ControllerContext = AsUser("U1")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.Open(999) as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result!.ActionName);
        Assert.Equal("Notification not found.", ctrl.TempData["ErrorMessage"]);
    }

    [Fact]
    public async Task MarkAllRead_MarksAllNotificationsAsRead()
    {
        // Arrange
        var user = new ApplicationUser { Id = "U1", UserName = "pilot" };

        var userManager = BuildUserManager();
        userManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

        var notificationRepo = new Mock<INotificationRepository>();
        notificationRepo.Setup(n => n.MarkAllAsReadAsync("U1")).Returns(Task.CompletedTask);

        var ctrl = new NotificationController(notificationRepo.Object, userManager.Object)
        {
            ControllerContext = AsUser("U1")
        };
        ctrl.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

        // Act
        var result = await ctrl.MarkAllRead() as RedirectToActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Index", result!.ActionName);
        notificationRepo.Verify(n => n.MarkAllAsReadAsync("U1"), Times.Once);
        Assert.Equal("All notifications marked as read.", ctrl.TempData["SuccessMessage"]);
    }
}

