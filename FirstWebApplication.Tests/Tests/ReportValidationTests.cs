using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FirstWebApplication.Models;
using Xunit;

namespace FirstWebApplication.Tests;

public class ReportValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void TooShortDescription_ShouldFail()
    {
        var r = new Report
        {
            ObstacleId = "CRN",
            Description = "short", // < 10
        };
        var results = Validate(r);
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Report.Description)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(25000)]
    public void Altitude_OutOfRange_ShouldFail(int val)
    {
        var r = new Report
        {
            ObstacleId = "CRN",
            Description = new string('x', 12),
            HeightFeet = (short)val
        };
        var results = Validate(r);
        Assert.Contains(results, v => v.MemberNames.Contains(nameof(Report.HeightFeet)));
    }

    [Fact]
    public void ValidReport_ShouldPass()
    {
        var r = new Report
        {
            ObstacleId = "CRN",
            Description = new string('x', 20),
            HeightFeet = 500
        };
        var results = Validate(r);
        Assert.Empty(results);
    }
}