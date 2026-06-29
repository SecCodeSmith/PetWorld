using Microsoft.AspNetCore.Identity;
using PetWorld.Application.Abstractions;
using PetWorld.Application.Dtos;

namespace PetWorld.Infrastructure.Identity;

/// <summary>
/// Implements the auth use-cases on top of ASP.NET Core Identity. Returns plain
/// DTOs so no Identity type escapes Infrastructure.
/// </summary>
public sealed class IdentityAuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(string email, string password, string displayName, CancellationToken ct = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? email : displayName.Trim(),
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return AuthResult.Fail(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return AuthResult.Success();
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe, CancellationToken ct = default)
    {
        var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        return result.Succeeded
            ? AuthResult.Success()
            : AuthResult.Fail("Nieprawidłowy e-mail lub hasło.");
    }

    public Task LogoutAsync(CancellationToken ct = default) => signInManager.SignOutAsync();

    public async Task<AccountProfile?> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        return new AccountProfile
        {
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
        };
    }

    public async Task<AuthResult> UpdateProfileAsync(string userId, AccountProfile profile, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthResult.Fail("Nie znaleziono użytkownika.");
        }

        user.DisplayName = profile.DisplayName?.Trim();
        user.PhoneNumber = profile.PhoneNumber?.Trim();

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? AuthResult.Success()
            : AuthResult.Fail(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
