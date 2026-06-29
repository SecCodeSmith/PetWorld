using System.Globalization;

namespace PetWorld.Application.Common;

/// <summary>
/// Formats a złoty amount as Polish złoty text, e.g. 289.00 -> "289 zł",
/// 129.99 -> "129,99 zł". Centralised so the catalogue, shop, product page, cart
/// and the AI advisor instructions all render prices identically.
/// </summary>
public static class PriceFormatter
{
    private static readonly CultureInfo Pl = new("pl-PL");

    public static string ToZl(decimal zl)
    {
        // Whole złoty render without decimals ("289 zł"); fractional with two ("129,99 zł").
        return zl == decimal.Truncate(zl)
            ? string.Create(Pl, $"{zl:0} zł")
            : string.Create(Pl, $"{zl:0.00} zł");
    }
}
