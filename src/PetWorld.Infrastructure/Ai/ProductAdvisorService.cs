using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using PetWorld.Application.Abstractions;
using PetWorld.Application.Dtos;
using PetWorld.Domain.Entities;
using PetWorld.Infrastructure.Persistence;

namespace PetWorld.Infrastructure.Ai;

/// <summary>
/// The AI advisor. Two Microsoft Agent Framework agents — a Writer and a Critic —
/// share one OpenAI chat client and are orchestrated by a hand-written loop (NOT the
/// workflow engine) so the control flow is fully explainable:
///
///   1. Writer produces an answer.
///   2. Critic returns a structured verdict (approved? + feedback).
///   3. If approved → done. If this was the 3rd iteration → give up (return as-is).
///      Otherwise the Writer revises using the feedback and we loop.
///
/// Every interaction is persisted for the history page.
/// </summary>
public sealed class ProductAdvisorService : IProductAdvisorService
{
    private const int MaxIterations = 3;

    private readonly AIAgent _writer;
    private readonly AIAgent _critic;
    private readonly IProductRepository _products;
    private readonly IChatHistoryRepository _history;
    private readonly ILogger<ProductAdvisorService> _logger;

    public ProductAdvisorService(
        IChatClient chatClient,
        IProductRepository products,
        IChatHistoryRepository history,
        ILogger<ProductAdvisorService> logger)
    {
        _products = products;
        _history = history;
        _logger = logger;

        var catalogue = Catalogue.ToInstructionText();
        _writer = chatClient.AsAIAgent(instructions: BuildWriterInstructions(catalogue), name: "Writer");
        _critic = chatClient.AsAIAgent(instructions: BuildCriticInstructions(catalogue), name: "Critic");
    }

    public async Task<AdvisorResult> GetAdviceAsync(string question, CancellationToken ct = default)
    {
        question = (question ?? string.Empty).Trim();

        var iteration = 1;
        var answer = await WriteAsync(question, ct);
        bool approved;

        while (true)
        {
            var verdict = await CritiqueAsync(question, answer, ct);
            if (verdict.Approved)
            {
                approved = true;
                _logger.LogInformation("Advisor answer approved after {Iterations} iteration(s).", iteration);
                break;
            }

            if (iteration >= MaxIterations)
            {
                approved = false;
                _logger.LogInformation("Advisor reached the {Max}-iteration cap without approval.", MaxIterations);
                break;
            }

            iteration++;
            answer = await ReviseAsync(question, answer, verdict.Feedback, ct);
        }

        await PersistAsync(question, answer, iteration, approved, ct);
        var recommended = await MatchRecommendedProductsAsync(answer, ct);

        return new AdvisorResult
        {
            Answer = answer,
            Iterations = iteration,
            Approved = approved,
            RecommendedProducts = recommended,
        };
    }

    private async Task<string> WriteAsync(string question, CancellationToken ct)
    {
        var response = await _writer.RunAsync(question, cancellationToken: ct);
        return response.Text.Trim();
    }

    private async Task<string> ReviseAsync(string question, string previousAnswer, string feedback, CancellationToken ct)
    {
        var response = await _writer.RunAsync(BuildRevisionPrompt(question, previousAnswer, feedback), cancellationToken: ct);
        return response.Text.Trim();
    }

    private async Task<CriticVerdict> CritiqueAsync(string question, string answer, CancellationToken ct)
    {
        var response = await _critic.RunAsync<CriticVerdict>(BuildCriticPrompt(question, answer), cancellationToken: ct);
        return response.Result;
    }

    private async Task<IReadOnlyList<Product>> MatchRecommendedProductsAsync(string answer, CancellationToken ct)
    {
        var all = await _products.GetAllAsync(ct);
        return all
            .Where(p => answer.Contains(p.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private Task PersistAsync(string question, string answer, int iterations, bool approved, CancellationToken ct) =>
        _history.AddAsync(new ChatInteraction
        {
            CreatedAt = DateTime.UtcNow,
            Question = question,
            FinalAnswer = answer,
            IterationCount = iterations,
            Approved = approved,
        }, ct);

    private static string BuildWriterInstructions(string catalogue) => $"""
        You are "Doradca PetWorld", a friendly and competent advisor for a Polish pet shop.
        Always answer in POLISH, in a warm but professional tone, in 2-4 short paragraphs.
        Recommend ONLY products from the catalogue below, using their EXACT name, and quote
        their EXACT price. Never invent products, prices or specifications. Recommend between
        1 and 3 products that genuinely fit the customer's question. Mention each recommended
        product's exact name in the body of the answer. If the question is unrelated to pets,
        politely steer the customer back to pet care.

        Catalogue (name — category — price):
        {catalogue}
        """;

    private static string BuildCriticInstructions(string catalogue) => $"""
        You review answers written by a Polish pet-shop advisor. Be strict but fair.
        Approve the answer ONLY if ALL of the following hold:
          - it is written in Polish and directly addresses the customer's question;
          - every recommended product exists in the catalogue below, written with its exact name;
          - every price mentioned matches the catalogue exactly;
          - the tone is friendly and professional and nothing is invented.
        If anything fails, do NOT approve, and provide specific, actionable feedback in Polish
        telling the writer exactly what to change.

        Catalogue (name — category — price):
        {catalogue}
        """;

    private static string BuildCriticPrompt(string question, string answer) => $"""
        Customer question:
        {question}

        Advisor answer:
        {answer}

        Decide whether to approve this answer.
        """;

    private static string BuildRevisionPrompt(string question, string previousAnswer, string feedback) => $"""
        Customer question:
        {question}

        Your previous answer:
        {previousAnswer}

        A reviewer did NOT approve it. Address ALL of this feedback and rewrite the answer in Polish:
        {feedback}
        """;
}
