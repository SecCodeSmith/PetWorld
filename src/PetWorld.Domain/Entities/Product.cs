namespace PetWorld.Domain.Entities;

/// <summary>
/// A catalogue product. The price is stored as <see cref="decimal"/> in złoty, which
/// is exact for money (no binary floating-point rounding issues like float/double).
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    /// <summary>Price in złoty (e.g. 289.00 = 289,00 zł).</summary>
    public decimal Price { get; set; }

    public string Description { get; set; } = string.Empty;
}
