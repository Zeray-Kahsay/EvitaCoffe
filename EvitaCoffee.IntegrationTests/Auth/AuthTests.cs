using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StackExchange.Redis;

namespace EvitaCoffee.IntegrationTests.Auth;

public class AuthTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Registration_Should_Create_User_And_Store_Otp()
    {
        var request = new
        {
            phoneNumber = "92234567",
            password = "StrongPass123!",
            fullName = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registration_Should_Store_Otp_In_Redis()
    {
        var request = new
        {
            phoneNumber = "92234567",
            password = "StrongPass123!",
            fullName = "Test User"
        };

        await _client.PostAsJsonAsync("/api/auth/register", request);

        var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = redis.GetDatabase();

        var key = "otp:+4791234567";

        var value = await db.StringGetAsync(key);

        value.IsNullOrEmpty.Should().BeFalse();

    }

    [Fact]
    public async Task VerifyPhone_Should_Confirm_User()
    {
        var phone = "91234567";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            phoneNumber = phone,
            password = "StrongPass123!",
            fullName = "Test User"
        });

        var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6378");
        var db = redis.GetDatabase();

        var key = "otp:+4791234567";
        var json = await db.StringGetAsync(key);

        var otpData = JsonSerializer.Deserialize<JsonElement>(json.ToString());
        var code = otpData.GetProperty("Code").GetString();

        var verifyResponse = await _client.PostAsJsonAsync("/api/auth/verify-phone", new
        {
            phoneNumber = phone,
            code = code
        });

        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_Befoe_Phone_Verification_Should_Fail()
    {
        var phone = "92345678";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            phoneNumber = phone,
            password = "StrongPass123!",
            fullName = "Test User"
        });

        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            phoneNumber = phone,
            password = "StrongPass123!"
        });

        login.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task All_Response_Should_Return_CorrelationId()
    {
        var response = await _client.GetAsync("/health/live");

        response.Headers.Contains("X-Correlation-ID").Should().BeTrue();
    }
}
