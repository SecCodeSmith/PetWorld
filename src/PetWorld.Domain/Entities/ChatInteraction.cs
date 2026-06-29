namespace PetWorld.Domain.Entities;

/// <summary>
/// One question/answer exchange with the AI advisor, persisted for the history page.
/// </summary>
public class ChatInteraction
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Question { get; set; } = string.Empty;

    public string FinalAnswer { get; set; } = string.Empty;

    /// <summary>How many Writer/Critic iterations produced the final answer (1-3).</summary>
    public int IterationCount { get; set; }

    /// <summary>True if the Critic approved the final answer within the iteration cap.</summary>
    public bool Approved { get; set; }
}
