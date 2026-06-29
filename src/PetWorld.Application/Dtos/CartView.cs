namespace PetWorld.Application.Dtos;

/// <summary>A single line in the cart (a product snapshot taken when it was added).</summary>
public sealed class CartLine
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public int UnitPriceGr { get; init; }

    public int Quantity { get; set; }

    public int LineTotalGr => UnitPriceGr * Quantity;
}

/// <summary>The whole cart shaped for display: lines plus computed totals.</summary>
public sealed class CartView
{
    public IReadOnlyList<CartLine> Lines { get; init; } = [];

    public int SubtotalGr { get; init; }

    public int ShippingGr { get; init; }

    public int TotalGr => SubtotalGr + ShippingGr;

    /// <summary>Total number of items (sum of quantities) — used for the nav badge.</summary>
    public int ItemCount { get; init; }

    public bool IsEmpty => Lines.Count == 0;
}
