using Microsoft.EntityFrameworkCore;
using PetWorld.Application.Abstractions;
using PetWorld.Domain.Entities;

namespace PetWorld.Infrastructure.Persistence;

public sealed class ChatHistoryRepository(PetWorldDbContext db) : IChatHistoryRepository
{
    public async Task<ChatInteraction> AddAsync(ChatInteraction interaction, CancellationToken ct = default)
    {
        db.ChatInteractions.Add(interaction);
        await db.SaveChangesAsync(ct);
        return interaction;
    }

    public async Task<IReadOnlyList<ChatInteraction>> ListNewestFirstAsync(CancellationToken ct = default) =>
        await db.ChatInteractions
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .ToListAsync(ct);
}
