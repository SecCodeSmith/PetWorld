using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetWorld.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> to build the model without a running
/// host. The connection string is only needed at design time; the real one is supplied
/// from configuration at runtime.
/// </summary>
public sealed class PetWorldDbContextFactory : IDesignTimeDbContextFactory<PetWorldDbContext>
{
    public PetWorldDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PetWorldDbContext>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=petworld;User=root;Password=root",
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new PetWorldDbContext(options);
    }
}
