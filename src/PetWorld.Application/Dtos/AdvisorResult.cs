using PetWorld.Domain.Entities;

namespace PetWorld.Application.Dtos;

/// <summary>
/// Result of one advisor request: the approved (or last) answer, how many
/// Writer/Critic iterations it took, whether the Critic approved it, and the
/// catalogue products the answer recommends (matched for the UI's ProductCards).
/// </summary>
public sealed class AdvisorResult
{
    public string Answer { get; init; } = string.Empty;

    public int Iterations { get; init; }

    public bool Approved { get; init; }

    public IReadOnlyList<Product> RecommendedProducts { get; init; } = [];
}
