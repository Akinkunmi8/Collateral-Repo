
using CollateralPortal.Application.Common.Constants;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Application.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollateralPortal.Infrastructure.Identity;

public sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(GetUsersRequest request,CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(user =>
                (user.Email != null &&
                 user.Email.Contains(search)) ||
                user.FullName.Contains(search));
        }

        query = query.OrderBy(user => user.FullName);

        var totalCount = await query.CountAsync(cancellationToken);

        List<ApplicationUser> users;

        if (request.All)
        {
            users = await query.ToListAsync(cancellationToken);
        }
        else
        {
            var pageNumber =
                Math.Max(request.PageNumber, 1);

            var pageSize =
                Math.Clamp(request.PageSize, 1, 100);

            users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                roles.FirstOrDefault() ?? string.Empty,
                user.IsActive));
        }

        var effectivePageNumber =
            request.All ? 1 : Math.Max(request.PageNumber, 1);

        var effectivePageSize =
            request.All
                ? Math.Max(totalCount, 1)
                : Math.Clamp(request.PageSize, 1, 100);

        var totalPages =
            request.All
                ? (totalCount == 0 ? 0 : 1)
                : (int)Math.Ceiling(
                    totalCount / (double)effectivePageSize);

        return new PagedResult<UserDto>(
            result,
            effectivePageNumber,
            effectivePageSize,
            totalCount,
            totalPages);
    }

    public async Task<UserDto?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
            return null;

        var roles =
            await _userManager.GetRolesAsync(user);

        return new UserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            roles.FirstOrDefault() ?? string.Empty,
            user.IsActive);
    }

    public IReadOnlyCollection<string> GetRoles()
    {
        return Roles.All;
    }

    public async Task<string> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRole(request.Role);

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
            FullName = request.FullName,
            IsActive = true,
            EmailConfirmed = true
        };

        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
            ThrowIdentityErrors(result);

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                request.Role);

        if (!roleResult.Succeeded)
        {
            // Prevent partially-created users.
            await _userManager.DeleteAsync(user);

            ThrowIdentityErrors(roleResult);
        }

        return user.Id;
    }

    public async Task UpdateAsync(
        string id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRole(request.Role);

        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"User '{id}' was not found.");
        }

        var emailOwner =
            await _userManager.FindByEmailAsync(request.Email);

        if (emailOwner is not null &&
            emailOwner.Id != user.Id)
        {
            throw new InvalidOperationException(
                "Another user already uses this email.");
        }

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            ThrowIdentityErrors(updateResult);

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        if (currentRoles.Count > 0 &&
            !currentRoles.Contains(request.Role))
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
                ThrowIdentityErrors(removeResult);
        }

        if (!currentRoles.Contains(request.Role))
        {
            var addResult =
                await _userManager.AddToRoleAsync(
                    user,
                    request.Role);

            if (!addResult.Succeeded)
                ThrowIdentityErrors(addResult);
        }
    }

    public async Task SetStatusAsync(
        string id,
        SetUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"User '{id}' was not found.");
        }

        user.IsActive = request.IsActive;

        user.LockoutEnd = request.IsActive
            ? null
            : DateTimeOffset.MaxValue;

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            ThrowIdentityErrors(result);
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"User '{id}' was not found.");
        }

        var result =
            await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            ThrowIdentityErrors(result);
    }

    private static void ValidateRole(string role)
    {
        if (!Roles.All.Contains(role))
        {
            throw new ArgumentException(
                $"Invalid role '{role}'.");
        }
    }

    private static void ThrowIdentityErrors(
        IdentityResult result)
    {
        var errors = string.Join(
            "; ",
            result.Errors.Select(error =>
                error.Description));

        throw new ArgumentException(errors);
    }
}