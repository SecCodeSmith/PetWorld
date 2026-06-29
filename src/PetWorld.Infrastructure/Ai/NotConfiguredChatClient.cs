using Microsoft.Extensions.AI;

namespace PetWorld.Infrastructure.Ai;

/// <summary>
/// Stand-in <see cref="IChatClient"/> registered when OPENAI_API_KEY is not set, so
/// the app still boots. Any real use surfaces a clear, friendly error instead of a
/// confusing startup crash or 401.
/// </summary>
internal sealed class NotConfiguredChatClient : IChatClient
{
    private const string Message =
        "Doradca AI jest niedostępny: brak skonfigurowanego klucza OPENAI_API_KEY.";

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Message);

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Message);

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
