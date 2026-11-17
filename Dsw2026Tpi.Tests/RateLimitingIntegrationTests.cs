using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Dsw2026Tpi.Api;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Dsw2026Tpi.Tests;

public class RateLimitingIntegrationTests : IClassFixture<RateLimitingWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;

    public RateLimitingIntegrationTests(RateLimitingWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminLogin_ShouldAllow5Requests_AndReturn429On6th()
    {
        var payload = new { email = "admin@test.com", password = "Password123!" };

        for (int i = 0; i < 5; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/admin/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var blockedResponse = await _client.PostAsJsonAsync("/api/auth/admin/login", payload);
        await AssertStandard429ResponseAsync(blockedResponse);
    }

    [Fact]
    public async Task PatientLogin_ShouldAllow10Requests_AndReturn429On11th()
    {
        var payload = new { email = "patient@test.com", dni = 30123456L };

        for (int i = 0; i < 10; i++)
        {
            var response = await _client.PostAsJsonAsync("/api/auth/patient/login", payload);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        var blockedResponse = await _client.PostAsJsonAsync("/api/auth/patient/login", payload);
        await AssertStandard429ResponseAsync(blockedResponse);
    }

    [Fact]
    public async Task AppointmentBooking_ShouldAllow5Requests_AndReturn429On6th()
    {
        static HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/appointments")
            {
                Content = JsonContent.Create(new { })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Test", "patient-booking-user");
            return request;
        }

        for (int i = 0; i < 5; i++)
        {
            using var request = CreateRequest();
            var response = await _client.SendAsync(request);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        using var blockedRequest = CreateRequest();
        var blockedResponse = await _client.SendAsync(blockedRequest);
        await AssertStandard429ResponseAsync(blockedResponse);
    }

    [Fact]
    public async Task DefaultPolicy_ShouldAllow100Requests_AndReturn429On101st()
    {
        static HttpRequestMessage CreateRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/specialties");
            request.Headers.Authorization = new AuthenticationHeaderValue("Test", "default-policy-user");
            return request;
        }

        for (int i = 0; i < 100; i++)
        {
            using var request = CreateRequest();
            var response = await _client.SendAsync(request);
            response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        using var blockedRequest = CreateRequest();
        var blockedResponse = await _client.SendAsync(blockedRequest);
        await AssertStandard429ResponseAsync(blockedResponse);
    }

    private static async Task AssertStandard429ResponseAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        root.GetProperty("errorCode").GetString().Should().Be("RATE_LIMIT_EXCEEDED");
        root.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();

        var details = root.GetProperty("details");
        details.GetArrayLength().Should().Be(1);
        details[0].GetProperty("field").GetString().Should().Be("request");
        details[0].GetProperty("issue").GetString().Should().Be("limit_exceeded");
    }
}

public class RateLimitingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<Application.Interfaces.IAuthenticationService>();
            services.AddScoped(_ => Substitute.For<Application.Interfaces.IAuthenticationService>());

            services.RemoveAll<Application.Interfaces.IAppointmentService>();
            services.AddScoped(_ => Substitute.For<Application.Interfaces.IAppointmentService>());

            services.RemoveAll<Application.Interfaces.ISpecialtyService>();
            services.AddScoped(_ => Substitute.For<Application.Interfaces.ISpecialtyService>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }
}

internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var clientName = authHeader.ToString().Replace("Test ", string.Empty).Trim();
        var claims = new[] { new Claim(ClaimTypes.Name, clientName) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}