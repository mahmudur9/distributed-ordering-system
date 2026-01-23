using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ProductService.API.Middlewares;

public class RemoteJwtAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public RemoteJwtAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        HttpClient httpClient,
        IMemoryCache cache)
        : base(options, logger, encoder)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return AuthenticateResult.NoResult();

        if (!header.ToString().StartsWith("Bearer "))
            return AuthenticateResult.Fail("Invalid Authorization header");

        var token = header.ToString()["Bearer ".Length..]; // Truncate the Bearer, then verify the token.

        // 1️⃣ Try cache first
        if (_cache.TryGetValue(token, out ClaimsPrincipal cachedPrincipal))
        {
            var cachedTicket =
                new AuthenticationTicket(cachedPrincipal, Scheme.Name);

            return AuthenticateResult.Success(cachedTicket);
        }

        // 2️⃣ Call auth service
        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:5252/api/Users/ValidateToken",
            new { token });

        if (!response.IsSuccessStatusCode)
            return AuthenticateResult.Fail("Token invalid");

        var authResponse =
            await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse is null || !authResponse.Valid)
            return AuthenticateResult.Fail("Token invalid");

        var claims = authResponse.Claims
            .Select(c => new Claim(c.Type, c.Value));

        var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // 3️⃣ Cache until token expiration
        var expiration = GetTokenExpiration(token);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };

        _cache.Set(token, principal, cacheOptions);

        return AuthenticateResult.Success(ticket);
    }

    private static DateTimeOffset GetTokenExpiration(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            return DateTimeOffset.FromUnixTimeSeconds(
                long.Parse(jwt.Claims.First(c => c.Type == "exp").Value));
        }
        catch
        {
            // fallback if token parsing fails
            return DateTimeOffset.UtcNow.AddMinutes(5);
        }
    }

    private record AuthResponse(bool Valid, IEnumerable<ClaimDto> Claims);
    private record ClaimDto(string Type, string Value);
}