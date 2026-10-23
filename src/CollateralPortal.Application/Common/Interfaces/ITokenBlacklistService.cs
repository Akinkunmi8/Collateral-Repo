namespace CollateralPortal.Application.Common.Interfaces;

public interface ITokenBlacklistService
{
    void BlacklistToken(string userId,string sessionId,DateTime expiresAtUtc);

    bool IsBlacklisted(string userId,string sessionId);
}