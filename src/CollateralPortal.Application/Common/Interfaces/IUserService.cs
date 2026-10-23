using CollateralPortal.Application.Common.Models;
using CollateralPortal.Application.Users;

namespace CollateralPortal.Application.Common.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAllAsync(GetUsersRequest request,CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(string id,CancellationToken cancellationToken = default);
    IReadOnlyCollection<string> GetRoles();
    Task<string> CreateAsync(CreateUserRequest request,CancellationToken cancellationToken = default);
    Task UpdateAsync(string id,UpdateUserRequest request,CancellationToken cancellationToken = default);
    Task SetStatusAsync(string id,SetUserStatusRequest request,CancellationToken cancellationToken = default);
    Task DeleteAsync(string id,CancellationToken cancellationToken = default);
}