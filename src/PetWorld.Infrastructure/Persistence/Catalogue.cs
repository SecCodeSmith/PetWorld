using PetWorld.Application.Common;
using PetWorld.Domain.Entities;

namespace PetWorld.Infrastructure.Persistence;

/// <summary>
/// The single source of truth for the 10 catalogue products. Used both to seed the
/// <c>products</c> table on startup AND to build the catalogue block embedded in the
/// AI advisor's instructions, so prices can never drift between the shop and the AI.
/// </summary>
public static class Catalogue
{
    public sealed record Item(string Name, string Category, int PriceGr, string Description);

    public static readonly IReadOnlyList<Item> Items =
    [
        new("Royal Canin Adult Dog 15kg", "Karma dla psów", 28900,
            "Pełnoporcjowa karma sucha dla dorosłych psów ras średnich. Wspiera trawienie i utrzymanie prawidłowej masy ciała."),
        new("Whiskas Adult Kurczak 7kg", "Karma dla kotów", 12900,
            "Sucha karma dla dorosłych kotów z kurczakiem. Kompletne, zbilansowane pożywienie na każdy dzień."),
        new("Tetra AquaSafe 500ml", "Akwarystyka", 4500,
            "Środek do uzdatniania wody akwariowej. Usuwa chlor i metale ciężkie oraz zabezpiecza śluzówkę ryb."),
        new("Trixie Drapak XL 150cm", "Akcesoria dla kotów", 39900,
            "Wysoki drapak z wieloma poziomami i domkiem. Idealny dla aktywnych kotów lubiących wspinaczkę."),
        new("Kong Classic Large", "Zabawki dla psów", 6900,
            "Wytrzymała gumowa zabawka dla psów. Można wypełnić ją smakołykami — świetna na nudę i stres."),
        new("Ferplast Klatka dla chomika", "Gryzonie", 18900,
            "Przestronna klatka dla chomika wraz z wyposażeniem. Łatwa w czyszczeniu i bezpieczna konstrukcja."),
        new("Flexi Smycz automatyczna 8m", "Akcesoria dla psów", 11900,
            "Automatyczna smycz taśmowa o długości 8 m. Wygodny uchwyt i niezawodny system hamowania."),
        new("Brit Premium Kitten 8kg", "Karma dla kotów", 15900,
            "Karma sucha dla kociąt wspierająca prawidłowy rozwój. Wysoka zawartość białka i tauryny."),
        new("JBL ProFlora CO2 Set", "Akwarystyka", 54900,
            "Kompletny zestaw nawożenia CO2 do akwarium roślinnego. Zapewnia bujny i zdrowy wzrost roślin."),
        new("Vitapol Siano dla królików 1kg", "Gryzonie", 2500,
            "Naturalne siano łąkowe dla królików i gryzoni. Wspiera prawidłowe trawienie i ścieranie zębów."),
    ];

    /// <summary>Fresh, untracked <see cref="Product"/> instances for seeding.</summary>
    public static List<Product> CreateSeedProducts() =>
        Items.Select(i => new Product
        {
            Name = i.Name,
            Category = i.Category,
            PriceGr = i.PriceGr,
            Description = i.Description,
        }).ToList();

    /// <summary>The catalogue rendered for the advisor's instructions (verbatim names + prices).</summary>
    public static string ToInstructionText() =>
        string.Join("\n", Items.Select(i => $"- {i.Name} — {i.Category} — {PriceFormatter.ToZl(i.PriceGr)}"));
}
