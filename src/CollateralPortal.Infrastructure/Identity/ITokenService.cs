namespace CollateralPortal.Infrastructure.Identity;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}
