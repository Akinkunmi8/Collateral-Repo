namespace CollateralPortal.Application.Authentication;

public sealed record RegisterRequest(string Email,string Password,string FullName,string Role);
public sealed record LoginRequest(string Email,string Password);
public sealed record AuthResponse(string Token, string Email, string FullName, string Role);