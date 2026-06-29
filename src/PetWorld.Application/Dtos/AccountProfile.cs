namespace PetWorld.Application.Dtos;

/// <summary>Editable account details shown on the profile page.</summary>
public sealed class AccountProfile
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}
