using PetWorld.Domain.Entities;

namespace PetWorld.Application.Common;

/// <summary>
/// Picks the catalogue products an answer recommends by matching product names in the
/// text. Shared so a freshly generated answer and a reopened one from history produce
/// the same recommended cards.
/// </summary>
public static class RecommendationMatcher
{
    public static IReadOnlyList<Product> Match(string? answer, IEnumerable<Product> catalogue)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return [];
        }

        return catalogue
            .Where(p => answer.Contains(p.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
