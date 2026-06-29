using PetWorld.Application.Dtos;

namespace PetWorld.Application.Abstractions;

/// <summary>
/// Authentication and account use-cases. The implementation wraps ASP.NET Core
/// Identity (SignInManager/UserManager) in Infrastructure so no Identity type
/// leaks into Application or the UI.
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, string displayName, CancellationToken ct = default);

    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe, CancellationToken ct = default);

    Task LogoutAsync(CancellationToken ct = default);

    Task<AccountProfile?> GetProfileAsync(string userId, CancellationToken ct = default);

    Task<AuthResult> UpdateProfileAsync(string userId, AccountProfile profile, CancellationToken ct = default);
}
