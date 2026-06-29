using Microsoft.AspNetCore.Identity;

namespace PetWorld.Infrastructure.Identity;

/// <summary>
/// Application user for ASP.NET Core Identity. Lives in Infrastructure so the
/// Identity type never leaks into Domain or Application.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Friendly name shown in the navbar and on the profile page.</summary>
    public string? DisplayName { get; set; }
}
