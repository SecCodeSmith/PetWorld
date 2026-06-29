namespace PetWorld.Application.Dtos;

/// <summary>A single line in the cart (a product snapshot taken when it was added).</summary>
public sealed class CartLine
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}

/// <summary>The whole cart shaped for display: lines plus computed totals.</summary>
public sealed class CartView
{
    public IReadOnlyList<CartLine> Lines { get; init; } = [];

    public decimal Subtotal { get; init; }

    public decimal Shipping { get; init; }

    public decimal Total => Subtotal + Shipping;

    /// <summary>Total number of items (sum of quantities) — used for the nav badge.</summary>
    public int ItemCount { get; init; }

    public bool IsEmpty => Lines.Count == 0;
}
