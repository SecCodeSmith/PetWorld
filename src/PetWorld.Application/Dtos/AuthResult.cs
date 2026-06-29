namespace PetWorld.Application.Dtos;

/// <summary>Outcome of an auth operation, free of any ASP.NET Core Identity types.</summary>
public sealed class AuthResult
{
    public bool Succeeded { get; init; }

    public string? Error { get; init; }

    public static AuthResult Success() => new() { Succeeded = true };

    public static AuthResult Fail(string error) => new() { Succeeded = false, Error = error };
}
