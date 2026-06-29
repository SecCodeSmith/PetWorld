using PetWorld.Application.Dtos;

namespace PetWorld.Application.Abstractions;

/// <summary>
/// The AI advisor. Runs the Writer/Critic loop and returns the final answer
/// together with recommended catalogue products.
/// </summary>
public interface IProductAdvisorService
{
    Task<AdvisorResult> GetAdviceAsync(string question, CancellationToken ct = default);
}
