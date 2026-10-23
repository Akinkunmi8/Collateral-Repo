using CollateralPortal.Application.Common.Constants;
using CollateralPortal.Application.Common.Interfaces;
using CollateralPortal.Application.Common.Models;
using CollateralPortal.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollateralPortal.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(
        IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetAll([FromQuery] GetUsersRequest request,CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(request,cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id,CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id,cancellationToken);
        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("roles")]
    public ActionResult<IReadOnlyCollection<string>> GetRoles()
    {
        return Ok(_userService.GetRoles());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request,CancellationToken cancellationToken)
    {
        var userId = await _userService.CreateAsync(request,cancellationToken);
        return CreatedAtAction(nameof(GetById),new { id = userId },new { id = userId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id,UpdateUserRequest request,CancellationToken cancellationToken)
    {
        await _userService.UpdateAsync(id,request,cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(string id,SetUserStatusRequest request,CancellationToken cancellationToken)
    {
        await _userService.SetStatusAsync(id,request,cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id,CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(id,cancellationToken);
        return NoContent();
    }
}