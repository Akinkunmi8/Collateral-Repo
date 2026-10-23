using CollateralPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CollateralPortal.Infrastructure.Authentication;

public sealed class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;

    public TokenBlacklistService(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string GetKey(string userId, string sessionId)
        => $"Blacklist:{userId}:{sessionId}";

    public void BlacklistToken(
        string userId,
        string sessionId,
        DateTime expiresAtUtc)
    {
        var key = GetKey(userId, sessionId);

        _cache.Set(
            key,
            true,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiresAtUtc
            });
    }

    public bool IsBlacklisted(string userId, string sessionId)
    {
        return _cache.TryGetValue(
            GetKey(userId, sessionId),
            out _);
    }
}