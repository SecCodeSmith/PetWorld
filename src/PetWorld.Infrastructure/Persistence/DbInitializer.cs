using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetWorld.Infrastructure.Identity;

namespace PetWorld.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and seeds the catalogue + demo user on startup. The migration
/// step retries so the app does not crash-loop while MySQL is still warming up.
/// </summary>
public static class DbInitializer
{
    private const int MaxMigrationAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

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

        db.Products.AddRange(Catalogue.CreateSeedProducts());
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} catalogue products.", Catalogue.Items.Count);
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
