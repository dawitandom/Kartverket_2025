using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FirstWebApplication.Models;
using FirstWebApplication.Models.ViewModel;
using Xunit;

namespace FirstWebApplication.Tests;

/// <summary>
/// Tester for brukervalidering og view models.
/// Verifiserer at brukerdata og registrering valideres korrekt.
/// </summary>
public class UserValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void RegisterViewModel_ValidData_ShouldPass()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void RegisterViewModel_EmptyUserName_ShouldFail()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            UserName = "",
            Email = "test@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(RegisterViewModel.UserName)));
    }

    [Fact]
    public void RegisterViewModel_InvalidEmail_ShouldFail()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            UserName = "testuser",
            Email = "not-an-email",  // Invalid email format
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(RegisterViewModel.Email)));
    }

    [Fact]
    public void RegisterViewModel_PasswordMismatch_ShouldFail()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "DifferentPass456!",  // Mismatch
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(RegisterViewModel.ConfirmPassword)));
    }

    [Fact]
    public void RegisterViewModel_EmptyPassword_ShouldFail()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "",
            ConfirmPassword = "",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(RegisterViewModel.Password)));
    }

    [Fact]
    public void CreateUserViewModel_ValidData_ShouldPass()
    {
        // Arrange
        var model = new CreateUserViewModel
        {
            UserName = "newadmin",
            Email = "admin@example.com",
            Password = "AdminPass123!",
            ConfirmPassword = "AdminPass123!",
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin"
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void CreateUserViewModel_EmptyRole_ShouldFail()
    {
        // Arrange
        var model = new CreateUserViewModel
        {
            UserName = "newadmin",
            Email = "admin@example.com",
            Password = "AdminPass123!",
            ConfirmPassword = "AdminPass123!",
            FirstName = "Admin",
            LastName = "User",
            Role = ""  // Empty role
        };

        // Act
        var results = Validate(model);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(CreateUserViewModel.Role)));
    }
}

