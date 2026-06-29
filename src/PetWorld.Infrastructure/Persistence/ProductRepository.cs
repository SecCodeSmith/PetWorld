using Microsoft.EntityFrameworkCore;
using PetWorld.Application.Abstractions;
using PetWorld.Domain.Entities;

namespace PetWorld.Infrastructure.Persistence;

public sealed class ProductRepository(PetWorldDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync(ct);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
}
