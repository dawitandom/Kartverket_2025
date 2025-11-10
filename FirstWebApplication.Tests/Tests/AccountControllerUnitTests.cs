using System.Threading.Tasks;
using FirstWebApplication.Controllers;
using FirstWebApplication.Models;
using FirstWebApplication.Tests.Fakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult; // <-- alias for å unngå konflikt
using Xunit;

namespace FirstWebApplication.Tests
{
    public class AccountControllerUnitTests
    {
        private static Mock<UserManager<ApplicationUser>> BuildUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task Login_MissingFields_ReturnsViewWithError()
        {
            var um = BuildUserManager().Object;
            var sm = new FakeSignInManager(um); // resultat spiller ingen rolle her
            var controller = new AccountController(um, sm);

            var result = await controller.Login("", "") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Username and password are required", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsViewWithError()
        {
            var um = BuildUserManager().Object;
            var sm = new FakeSignInManager(um) { NextResult = SignInResult.Failed };
            var controller = new AccountController(um, sm);

            var result = await controller.Login("pilot", "wrong") as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Invalid username or password", controller.ViewBag.Error);
        }

        [Fact]
        public async Task Login_ValidPassword_RedirectsToHome()
        {
            var um = BuildUserManager().Object;
            var sm = new FakeSignInManager(um) { NextResult = SignInResult.Success };
            var controller = new AccountController(um, sm);

            var result = await controller.Login("pilot", "okpass") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ActionName);
            Assert.Equal("Home", result.ControllerName);
        }
    }
}