namespace CollateralPortal.Application.Users;

public sealed record UserDto(string Id,string Email,string FullName,string Role,bool IsActive);
public sealed record CreateUserRequest(string Email,string Password,string FullName,string Role);
public sealed record UpdateUserRequest(string Email,string FullName,string Role);
public sealed record SetUserStatusRequest(bool IsActive);

public sealed record GetUsersRequest
{
    public string? Search { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public bool All { get; init; } = false;
}