using CollateralPortal.Application.Authentication;
using CollateralPortal.Application.Common.Constants;
using CollateralPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CollateralPortal.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ITokenBlacklistService tokenBlacklistService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _tokenBlacklistService = tokenBlacklistService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Roles.All.Contains(request.Role))
        {
            throw new ArgumentException(
                $"Unknown role '{request.Role}'. " +
                $"Valid roles: {string.Join(", ", Roles.All)}");
        }

        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(x => x.Description));

            throw new ArgumentException(errors);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, request.Role);

        if (!roleResult.Succeeded)
        {
            // Avoid leaving a partially registered user behind.
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(x => x.Description));

            throw new InvalidOperationException(errors);
        }

        var token =
            await _tokenService.GenerateTokenAsync(user);

        return new AuthResponse(
            token,
            user.Email!,
            user.FullName,
            request.Role);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user is null ||
            !user.IsActive ||
            !await _userManager.CheckPasswordAsync(
                user,
                request.Password))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var token =
            await _tokenService.GenerateTokenAsync(user);

        return new AuthResponse(
            token,
            user.Email!,
            user.FullName,
            roles.FirstOrDefault() ?? string.Empty);
    }

    public Task LogoutAsync(string userId,string sessionId,CancellationToken cancellationToken = default)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        _tokenBlacklistService.BlacklistToken(userId,sessionId,expiresAtUtc);

        return Task.CompletedTask;
    }
}