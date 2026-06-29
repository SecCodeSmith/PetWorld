using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PetWorld.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> to build the model without a running host.
/// The connection string is read from configuration (environment variables / command line),
/// exactly like at runtime — never hardcoded. A localhost default is used ONLY when nothing is
/// supplied, so a bare <c>dotnet ef</c> run still works without exposing real credentials.
/// </summary>
public sealed class PetWorldDbContextFactory : IDesignTimeDbContextFactory<PetWorldDbContext>
{
    // Local-only fallback for a bare `dotnet ef` run with no configuration present.
    // Real connection strings come from configuration (e.g. ConnectionStrings__Default).
    private const string FallbackConnectionString =
        "Server=localhost;Port=3306;Database=petworld;User=root;Password=root";

    public PetWorldDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var connectionString = configuration.GetConnectionString("Default") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<PetWorldDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new PetWorldDbContext(options);
    }
}
