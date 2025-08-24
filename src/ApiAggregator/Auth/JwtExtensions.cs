using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiAggregator.Auth;

public record AuthOptions
{
    public bool Enabled { get; set; }
    public string Issuer { get; set; } = "ApiAggregator";
    public string Audience { get; set; } = "ApiAggregator";
    public string SigningKey { get; set; } = default!;
}

public interface IJwtTokenService
{
    string IssueToken(string user, TimeSpan ttl);
}

internal class JwtTokenService : IJwtTokenService
{
    private readonly AuthOptions _opts;
    public JwtTokenService(Microsoft.Extensions.Options.IOptions<AuthOptions> opts) => _opts = opts.Value;

    public string IssueToken(string user, TimeSpan ttl)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTimeOffset.UtcNow;
        var jwt = new JwtSecurityToken(
        issuer: _opts.Issuer,
        audience: _opts.Audience,
        claims: new[] { new Claim(ClaimTypes.Name, user) },
        notBefore: now.UtcDateTime,
        expires: now.Add(ttl).UtcDateTime,
        signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, AuthOptions opts)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = opts.Issuer,
                ValidateAudience = true,
                ValidAudience = opts.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.SigningKey)),
                ValidateLifetime = true
            };
        });
        services.AddAuthorization();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        return services;
    }
}