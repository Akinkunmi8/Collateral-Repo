using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CollateralPortal.Application.Authentication;
using CollateralPortal.Application.Common.Constants;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollateralPortal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Register(RegisterRequest request,CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request,cancellationToken);

        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request,CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request,cancellationToken);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
    CancellationToken cancellationToken)
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var sessionId =
            User.FindFirstValue("SessionId");

        if (string.IsNullOrWhiteSpace(userId) ||
            string.IsNullOrWhiteSpace(sessionId))
        {
            return Unauthorized(new
            {
                error = "Invalid authentication session."
            });
        }

        await _authService.LogoutAsync(userId,sessionId,cancellationToken);

        return Ok(new
        {
            message = "Logout successful."
        });
    }
}