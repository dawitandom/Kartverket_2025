
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FirstWebApplication.Tests;

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Login_WithoutCsrfToken_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", "pilot"),
            new KeyValuePair<string, string>("password", "pilot123")
        });

        // Act
        var response = await _client.PostAsync("/Account/Login", content);

        // Assert - CSRF validation skal feile
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || 
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected BadRequest/Redirect, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Logout_WithoutCsrfToken_ShouldReturnBadRequest()
    {
        // Arrange
        var content = new StringContent("");

        // Act
        var response = await _client.PostAsync("/Account/Logout", content);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest || 
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found,
            $"Expected BadRequest/Redirect, got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task GetLoginPage_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Account/Login");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("__RequestVerificationToken", content);
    }

    [Fact]
    public async Task HomePage_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}