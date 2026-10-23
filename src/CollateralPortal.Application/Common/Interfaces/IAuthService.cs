using CollateralPortal.Application.Authentication;

namespace CollateralPortal.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string userId,
        string sessionId,
        CancellationToken cancellationToken = default);
}