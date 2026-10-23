using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CollateralPortal.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public TokenService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var sessionId = Guid.NewGuid().ToString();

        var claims = new List<Claim>
    {
         new Claim(JwtRegisteredClaimNames.Sub,user.Id),

        new Claim(ClaimTypes.NameIdentifier,user.Id),

        new Claim(JwtRegisteredClaimNames.Email,user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name,user.FullName ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti,sessionId),
        new Claim("SessionId",sessionId)
    };
        claims.AddRange(roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
