using System.Threading.Tasks;
using FirstWebApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FirstWebApplication.Tests.Fakes
{
    public class FakeSignInManager : SignInManager<ApplicationUser>
    {
        public SignInResult NextResult { get; set; } = SignInResult.Success;

        public FakeSignInManager(UserManager<ApplicationUser> userManager)
            : base(userManager,
                new HttpContextAccessor(),
                new UserClaimsPrincipalFactory<ApplicationUser>(
                    userManager,
                    global::Microsoft.Extensions.Options.Options.Create(new IdentityOptions())
                ),
                global::Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
                new LoggerFactory().CreateLogger<SignInManager<ApplicationUser>>(),
                new AuthenticationSchemeProvider(
                    global::Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions())
                ),
                new DefaultUserConfirmation<ApplicationUser>())
        { }

        public override Task<SignInResult> PasswordSignInAsync(
            string userName, string password, bool isPersistent, bool lockoutOnFailure)
            => Task.FromResult(NextResult);
    }
}