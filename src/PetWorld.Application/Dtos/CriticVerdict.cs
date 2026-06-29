namespace PetWorld.Application.Dtos;

/// <summary>
/// Structured verdict returned by the Critic agent. Deserialised by the Agent
/// Framework via RunAsync&lt;CriticVerdict&gt; (properties must be settable).
/// </summary>
public sealed class CriticVerdict
{
    /// <summary>True when the Writer's answer is relevant, on-catalogue and correctly priced.</summary>
    public bool Approved { get; set; }

    /// <summary>Actionable feedback for the Writer when not approved.</summary>
    public string Feedback { get; set; } = string.Empty;
}
