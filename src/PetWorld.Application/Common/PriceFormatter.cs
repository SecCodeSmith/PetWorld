using System.Globalization;

namespace PetWorld.Application.Common;

/// <summary>
/// Formats money stored in grosze as Polish złoty text, e.g. 28900 -> "289 zł",
/// 12999 -> "129,99 zł". Centralised so the catalogue, shop, product page, cart
/// and the AI advisor instructions all render prices identically.
/// </summary>
public static class PriceFormatter
{
    private static readonly CultureInfo Pl = new("pl-PL");

    public static string ToZl(int grosze)
    {
        if (grosze % 100 == 0)
        {
            return string.Create(Pl, $"{grosze / 100} zł");
        }

        decimal zl = grosze / 100m;
        return string.Create(Pl, $"{zl:0.00} zł");
    }
}
