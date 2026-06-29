using PetWorld.Application.Dtos;

namespace PetWorld.Web.Services;

/// <summary>
/// Holds the current advisor conversation. Registered Scoped, so it lives for the whole
/// Blazor circuit — the chat survives navigating away and back, and can be reopened.
/// </summary>
public sealed class ChatSessionState
{
    public List<ChatExchange> Exchanges { get; } = [];

    public bool IsEmpty => Exchanges.Count == 0;

    public bool IsBusy => Exchanges.Exists(e => e.Loading);

    public void Clear() => Exchanges.Clear();
}

/// <summary>One turn in the conversation: the question and its (pending) answer.</summary>
public sealed class ChatExchange
{
    public required string Question { get; init; }

    public AdvisorResult? Result { get; set; }

    public string? Error { get; set; }

    public bool Loading { get; set; }
}
