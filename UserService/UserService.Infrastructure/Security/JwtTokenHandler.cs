using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions.Security;
using UserService.Application.Requests;
using UserService.Application.Responses;

namespace UserService.Infrastructure.Security;

public class JwtTokenHandler : ITokenHandler
{
    private readonly IConfiguration _configuration;

    public JwtTokenHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateToken(Guid userId, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(_configuration.GetSection("SecretKey").Value!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(type: "userId", value: userId.ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            ]),
            Expires = DateTime.UtcNow.AddHours(int.Parse(_configuration.GetSection("TokenExpiryTimeInHours").Value!)),
            SigningCredentials =
                new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public TokenValidationResponse ValidateToken(TokenValidationRequest request)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            
            var principal = handler.ValidateToken(
                request.Token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration.GetSection("SecretKey").Value!)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                },
                out _);

            var claims = principal.Claims.Select(c => new {c.Type, c.Value});
            
            var claimResponses = new List<ClaimResponse>();
            foreach (var claim in claims)
            {
                var claimResponse = new ClaimResponse()
                {
                    Type = claim.Type,
                    Value = claim.Value
                };
                claimResponses.Add(claimResponse);
            }
            var response = new TokenValidationResponse()
            {
                Valid = true,
                Claims = claimResponses
            };
            return response;
        }
        catch
        {
            throw new UnauthorizedAccessException("Invalid token!");
        }
    }
    
    /*public TokenValidationResponse GetClaims()
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            string token = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"]!;
            var jwt = handler.ReadJwtToken(token.Split("Bearer ")[1]);

            var claims = jwt.Claims.Select(c => (c.Type, c.Value));

            var claimResponses = new List<ClaimResponse>();
            foreach (var claim in claims)
            {
                var claimResponse = new ClaimResponse()
                {
                    Type = claim.Type,
                    Value = claim.Value
                };
                claimResponses.Add(claimResponse);
            }
            var response = new TokenValidationResponse()
            {
                Valid = true,
                Claims = claimResponses
            };
            return response;
        }
        catch (Exception ex)
        {
            throw;
        }
    }*/
}