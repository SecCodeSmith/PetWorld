namespace PetWorld.Application.Dtos;

/// <summary>
/// Structured intent classification returned by the Router agent. Deserialised by the
/// Agent Framework via RunAsync&lt;QueryIntent&gt; (properties must be settable).
/// </summary>
public sealed class QueryIntent
{
    /// <summary>
    /// True when the customer's message is a genuine request for product or pet-care advice
    /// that the shop can answer from its catalogue; false for greetings, small talk,
    /// empty/gibberish input or anything off-topic.
    /// </summary>
    public bool IsProductQuery { get; set; }
}
