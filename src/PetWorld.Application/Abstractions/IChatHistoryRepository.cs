using PetWorld.Domain.Entities;

namespace PetWorld.Application.Abstractions;

/// <summary>Persists and lists advisor interactions for the history page.</summary>
public interface IChatHistoryRepository
{
    Task<ChatInteraction> AddAsync(ChatInteraction interaction, CancellationToken ct = default);

    /// <summary>All interactions, newest first.</summary>
    Task<IReadOnlyList<ChatInteraction>> ListNewestFirstAsync(CancellationToken ct = default);

    /// <summary>A single interaction by id (used to reopen a past chat), or null.</summary>
    Task<ChatInteraction?> GetByIdAsync(int id, CancellationToken ct = default);
}
