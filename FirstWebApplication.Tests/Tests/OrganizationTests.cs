using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FirstWebApplication.Models;
using Xunit;

namespace FirstWebApplication.Tests;

/// <summary>
/// Tester for organisasjonsmodeller og validering.
/// Verifiserer at organisasjonsdata valideres korrekt.
/// </summary>
public class OrganizationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Organization_ValidData_ShouldPass()
    {
        // Arrange
        var org = new Organization
        {
            OrganizationId = 1,
            Name = "Norsk Luftambulanse",
            ShortCode = "NLA"
        };

        // Act
        var results = Validate(org);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Organization_EmptyName_ShouldFail()
    {
        // Arrange
        var org = new Organization
        {
            OrganizationId = 1,
            Name = "",  // Empty name
            ShortCode = "NLA"
        };

        // Act
        var results = Validate(org);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Organization.Name)));
    }

    [Fact]
    public void Organization_EmptyShortCode_ShouldFail()
    {
        // Arrange
        var org = new Organization
        {
            OrganizationId = 1,
            Name = "Test Organization",
            ShortCode = ""  // Empty short code
        };

        // Act
        var results = Validate(org);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Organization.ShortCode)));
    }

    [Fact]
    public void Organization_TooLongShortCode_ShouldFail()
    {
        // Arrange - ShortCode has MaxLength(10)
        var org = new Organization
        {
            OrganizationId = 1,
            Name = "Test Organization",
            ShortCode = "VERYLONGCODE123"  // > 10 characters
        };

        // Act
        var results = Validate(org);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Organization.ShortCode)));
    }

    [Fact]
    public void Organization_TooLongName_ShouldFail()
    {
        // Arrange - Name has MaxLength(100)
        var org = new Organization
        {
            OrganizationId = 1,
            Name = new string('A', 101),  // 101 characters > 100
            ShortCode = "TEST"
        };

        // Act
        var results = Validate(org);

        // Assert
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Organization.Name)));
    }
}

