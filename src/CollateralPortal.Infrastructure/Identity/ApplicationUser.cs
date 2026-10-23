using Microsoft.AspNetCore.Identity;

namespace CollateralPortal.Infrastructure.Identity;

/// <summary>Extends the standard Identity user with the fields the portal needs on staff accounts.</summary>
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}
