namespace PetWorld.Domain.Entities;

/// <summary>
/// A catalogue product. Prices are stored in grosze (1 zł = 100 gr) as an integer
/// to avoid floating-point rounding issues with money.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    /// <summary>Price in grosze (e.g. 28900 = 289,00 zł).</summary>
    public int PriceGr { get; set; }

    public string Description { get; set; } = string.Empty;
}
