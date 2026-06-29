using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetWorld.Domain.Entities;
using PetWorld.Infrastructure.Identity;

namespace PetWorld.Infrastructure.Persistence;

/// <summary>
/// Single EF Core context for the whole app: the domain tables (products,
/// chat_interactions) AND the ASP.NET Core Identity tables. One connection, one
/// migration history and one transactional store — see the README for the rationale.
/// </summary>
public class PetWorldDbContext : IdentityDbContext<ApplicationUser>
{
    public PetWorldDbContext(DbContextOptions<PetWorldDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<ChatInteraction> ChatInteractions => Set<ChatInteraction>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // configures the Identity tables

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Price).HasPrecision(10, 2).IsRequired(); // DECIMAL(10,2), exact for money

            entity.Property(p => p.Description).IsRequired(); // maps to longtext
        });

        builder.Entity<ChatInteraction>(entity =>
        {
            entity.ToTable("chat_interactions");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CreatedAt).IsRequired();
            entity.Property(c => c.Question).IsRequired();     // longtext
            entity.Property(c => c.FinalAnswer).IsRequired();  // longtext
            entity.Property(c => c.IterationCount).IsRequired();
            entity.Property(c => c.Approved).IsRequired();
            entity.HasIndex(c => c.CreatedAt); // history is ordered newest-first
        });
    }
}
