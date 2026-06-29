using PetWorld.Domain.Entities;

namespace PetWorld.Application.Abstractions;

/// <summary>Read access to the product catalogue.</summary>
public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
}
