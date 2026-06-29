using PetWorld.Application.Abstractions;
using PetWorld.Application.Dtos;

namespace PetWorld.Infrastructure.Cart;

/// <summary>
/// In-memory cart, registered Scoped so it lives for the duration of one Blazor
/// circuit (the user's connection). Lines store a product snapshot taken at add-time
/// so rendering the cart needs no further database access. See README for limitations.
/// </summary>
public sealed class CartService(IProductRepository products) : ICartService
{
    /// <summary>Flat delivery fee applied while the cart has at least one item (15 zł).</summary>
    private const int ShippingGr = 1500;

    private readonly Dictionary<int, CartLine> _lines = new();

    public event Action? Changed;

    public async Task AddAsync(int productId, int quantity = 1, CancellationToken ct = default)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        if (_lines.TryGetValue(productId, out var existing))
        {
            existing.Quantity += quantity;
            Changed?.Invoke();
            return;
        }

        var product = await products.GetByIdAsync(productId, ct);
        if (product is null)
        {
            return;
        }

        _lines[productId] = new CartLine
        {
            ProductId = product.Id,
            Name = product.Name,
            Category = product.Category,
            UnitPriceGr = product.PriceGr,
            Quantity = quantity,
        };
        Changed?.Invoke();
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        if (!_lines.TryGetValue(productId, out var line))
        {
            return;
        }

        if (quantity < 1)
        {
            _lines.Remove(productId);
        }
        else
        {
            line.Quantity = quantity;
        }

        Changed?.Invoke();
    }

    public void Remove(int productId)
    {
        if (_lines.Remove(productId))
        {
            Changed?.Invoke();
        }
    }

    public void Clear()
    {
        if (_lines.Count == 0)
        {
            return;
        }

        _lines.Clear();
        Changed?.Invoke();
    }

    public CartView GetCart()
    {
        var lines = _lines.Values.OrderBy(l => l.Name).ToList();
        var subtotal = lines.Sum(l => l.LineTotalGr);
        return new CartView
        {
            Lines = lines,
            SubtotalGr = subtotal,
            ShippingGr = lines.Count == 0 ? 0 : ShippingGr,
            ItemCount = lines.Sum(l => l.Quantity),
        };
    }

    public int ItemCount => _lines.Values.Sum(l => l.Quantity);
}
