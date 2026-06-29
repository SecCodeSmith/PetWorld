using PetWorld.Application.Dtos;

namespace PetWorld.Application.Abstractions;

/// <summary>
/// Shopping cart. The implementation is an in-memory store scoped to the current
/// Blazor circuit (see the README for the rationale and limitations).
/// </summary>
public interface ICartService
{
    /// <summary>Raised whenever the cart contents change, so the nav badge can refresh live.</summary>
    event Action? Changed;

    /// <summary>Adds a product (looked up by id) to the cart, or increases its quantity.</summary>
    Task AddAsync(int productId, int quantity = 1, CancellationToken ct = default);

    void UpdateQuantity(int productId, int quantity);

    void Remove(int productId);

    void Clear();

    CartView GetCart();

    /// <summary>Sum of quantities across all lines — used for the nav cart badge.</summary>
    int ItemCount { get; }
}
