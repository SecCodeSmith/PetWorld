using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetWorld.Domain.Entities;
using PetWorld.Infrastructure.Identity;

namespace PetWorld.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and seeds the catalogue + demo user on startup. The migration
/// step retries so the app does not crash-loop while MySQL is still warming up.
///
/// The 10 catalogue products are defined here so the database is the single source of
/// truth: the shop reads them from the <c>products</c> table and the AI advisor reads
/// them through <see cref="IProductRepository"/>, so prices can never drift between the
/// two. Product names/descriptions are customer-facing copy, hence Polish.
/// </summary>
public static class DbInitializer
{
    private const int MaxMigrationAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private static readonly (string Name, string Category, decimal Price, string Description)[] SeedProducts =
    [
        ("Royal Canin Adult Dog 15kg", "Karma dla psów", 289.00m,
            "Pełnoporcjowa karma sucha dla dorosłych psów ras średnich. Wspiera trawienie i utrzymanie prawidłowej masy ciała."),
        ("Whiskas Adult Kurczak 7kg", "Karma dla kotów", 129.00m,
            "Sucha karma dla dorosłych kotów z kurczakiem. Kompletne, zbilansowane pożywienie na każdy dzień."),
        ("Tetra AquaSafe 500ml", "Akwarystyka", 45.00m,
            "Środek do uzdatniania wody akwariowej. Usuwa chlor i metale ciężkie oraz zabezpiecza śluzówkę ryb."),
        ("Trixie Drapak XL 150cm", "Akcesoria dla kotów", 399.00m,
            "Wysoki drapak z wieloma poziomami i domkiem. Idealny dla aktywnych kotów lubiących wspinaczkę."),
        ("Kong Classic Large", "Zabawki dla psów", 69.00m,
            "Wytrzymała gumowa zabawka dla psów. Można wypełnić ją smakołykami — świetna na nudę i stres."),
        ("Ferplast Klatka dla chomika", "Gryzonie", 189.00m,
            "Przestronna klatka dla chomika wraz z wyposażeniem. Łatwa w czyszczeniu i bezpieczna konstrukcja."),
        ("Flexi Smycz automatyczna 8m", "Akcesoria dla psów", 119.00m,
            "Automatyczna smycz taśmowa o długości 8 m. Wygodny uchwyt i niezawodny system hamowania."),
        ("Brit Premium Kitten 8kg", "Karma dla kotów", 159.00m,
            "Karma sucha dla kociąt wspierająca prawidłowy rozwój. Wysoka zawartość białka i tauryny."),
        ("JBL ProFlora CO2 Set", "Akwarystyka", 549.00m,
            "Kompletny zestaw nawożenia CO2 do akwarium roślinnego. Zapewnia bujny i zdrowy wzrost roślin."),
        ("Vitapol Siano dla królików 1kg", "Gryzonie", 25.00m,
            "Naturalne siano łąkowe dla królików i gryzoni. Wspiera prawidłowe trawienie i ścieranie zębów."),
    ];

    public static async Task InitializeAsync(IServiceProvider rootProvider, CancellationToken ct = default)
    {
        using var scope = rootProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var db = sp.GetRequiredService<PetWorldDbContext>();

        await MigrateWithRetryAsync(db, logger, ct);
        await SeedProductsAsync(db, logger, ct);
        await SeedDemoUserAsync(sp, logger, ct);
    }

    private static async Task MigrateWithRetryAsync(PetWorldDbContext db, ILogger logger, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync(ct);
                logger.LogInformation("Database migrations applied.");
                return;
            }
            catch (Exception ex)
            {
                if (attempt >= MaxMigrationAttempts)
                {
                    logger.LogError(ex, "Could not apply migrations after {Attempts} attempts.", MaxMigrationAttempts);
                    throw;
                }

                logger.LogWarning(
                    "Database not ready (attempt {Attempt}/{Max}): {Error}. Retrying in {Delay}s.",
                    attempt, MaxMigrationAttempts, ex.Message, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, ct);
            }
        }
    }

    private static async Task SeedProductsAsync(PetWorldDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.Products.AnyAsync(ct))
        {
            return;
        }

        db.Products.AddRange(SeedProducts.Select(p => new Product
        {
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            Description = p.Description,
        }));
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} catalogue products.", SeedProducts.Length);
    }

    private static async Task SeedDemoUserAsync(IServiceProvider sp, ILogger logger, CancellationToken ct)
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var email = config["SEED_USER_EMAIL"];
        var password = config["SEED_USER_PASSWORD"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("No seed user configured (SEED_USER_EMAIL/SEED_USER_PASSWORD); skipping.");
            return;
        }

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Demo",
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            logger.LogInformation("Seeded demo user {Email}.", email);
        }
        else
        {
            logger.LogWarning("Could not seed demo user: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
