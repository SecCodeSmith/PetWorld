using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using PetWorld.Application.Abstractions;
using PetWorld.Application.Common;
using PetWorld.Application.Dtos;
using PetWorld.Domain.Entities;

namespace PetWorld.Infrastructure.Ai;

/// <summary>
/// The AI advisor. A lightweight Router first classifies the customer's intent; only a genuine
/// product/pet-advice request runs the Writer/Critic loop, everything else (greetings, small
/// talk, off-topic, gibberish) gets a fixed invitation. Three Microsoft Agent Framework agents
/// share one OpenAI chat client, orchestrated by a hand-written loop (NOT the workflow engine)
/// so the control flow is fully explainable:
///
///   0. Router decides whether the message is a product question. If not → fixed reply, done.
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

    /// <summary>
    /// Deterministic reply for messages that are not product/pet-advice requests (greetings,
    /// small talk, gibberish, off-topic). Returned verbatim from code — never model-generated —
    /// so the wording is always correct even with a small/weak model.
    /// </summary>
    private const string OffTopicReply =
        "Cześć! Z przyjemnością pomogę Ci w zrozumieniu naszej oferty — po prostu zapytaj.";

    private readonly IChatClient _chatClient;
    private readonly IProductRepository _products;
    private readonly IChatHistoryRepository _history;
    private readonly ILogger<ProductAdvisorService> _logger;

    public ProductAdvisorService(
        IChatClient chatClient,
        IProductRepository products,
        IChatHistoryRepository history,
        ILogger<ProductAdvisorService> logger)
    {
        _chatClient = chatClient;
        _products = products;
        _history = history;
        _logger = logger;
    }

    public async Task<AdvisorResult> GetAdviceAsync(string question, CancellationToken ct = default)
    {
        question = (question ?? string.Empty).Trim();

        // Empty input never reaches the model — answer with the standard invitation.
        if (question.Length == 0)
        {
            return await OffTopicAsync(question, ct);
        }

        // The catalogue is read from the database (the single source of truth, seeded by
        // DbInitializer) and baked into the agents' instructions, so the advisor can never
        // recommend a product or price that is not in the shop.
        var products = await _products.GetAllAsync(ct);
        var catalogue = BuildCatalogueText(products);

        // Intent gate: a small/weak model writes garbled prose if forced to "recommend" for an
        // off-topic message, so we first classify the intent (a robust yes/no) and only run the
        // Writer/Critic loop for genuine product questions. Everything else gets the fixed reply.
        if (!await IsProductQueryAsync(catalogue, question, ct))
        {
            _logger.LogInformation("Advisor classified the message as off-topic; returning the standard invitation.");
            return await OffTopicAsync(question, ct);
        }

        var writer = _chatClient.AsAIAgent(instructions: BuildWriterInstructions(catalogue), name: "Writer");
        var critic = _chatClient.AsAIAgent(instructions: BuildCriticInstructions(catalogue), name: "Critic");

        var iteration = 1;
        var answer = await WriteAsync(writer, question, ct);
        bool approved;

        while (true)
        {
            var verdict = await CritiqueAsync(critic, question, answer, ct);
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
            answer = await ReviseAsync(writer, question, answer, verdict.Feedback, ct);
        }

        await PersistAsync(question, answer, iteration, approved, ct);

        return new AdvisorResult
        {
            Answer = answer,
            Iterations = iteration,
            Approved = approved,
            RecommendedProducts = RecommendationMatcher.Match(answer, products),
        };
    }

    private static async Task<string> WriteAsync(AIAgent writer, string question, CancellationToken ct)
    {
        var response = await writer.RunAsync(question, cancellationToken: ct);
        return response.Text.Trim();
    }

    private static async Task<string> ReviseAsync(AIAgent writer, string question, string previousAnswer, string feedback, CancellationToken ct)
    {
        var response = await writer.RunAsync(BuildRevisionPrompt(question, previousAnswer, feedback), cancellationToken: ct);
        return response.Text.Trim();
    }

    private static async Task<CriticVerdict> CritiqueAsync(AIAgent critic, string question, string answer, CancellationToken ct)
    {
        var response = await critic.RunAsync<CriticVerdict>(BuildCriticPrompt(question, answer), cancellationToken: ct);
        return response.Result;
    }

    private static string BuildCatalogueText(IReadOnlyList<Product> products) =>
        string.Join("\n", products.Select(p => $"- {p.Name} — {p.Category} — {PriceFormatter.ToZl(p.Price)}"));

    private async Task<bool> IsProductQueryAsync(string catalogue, string question, CancellationToken ct)
    {
        var router = _chatClient.AsAIAgent(instructions: BuildRouterInstructions(catalogue), name: "Router");
        var response = await router.RunAsync<QueryIntent>(question, cancellationToken: ct);
        return response.Result.IsProductQuery;
    }

    private async Task<AdvisorResult> OffTopicAsync(string question, CancellationToken ct)
    {
        await PersistAsync(question, OffTopicReply, iterations: 1, approved: true, ct);
        return new AdvisorResult
        {
            Answer = OffTopicReply,
            Iterations = 1,
            Approved = true,
            RecommendedProducts = [],
        };
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

    private static string BuildRouterInstructions(string catalogue) => $"""
        You triage a customer's message for "PetWorld", a Polish pet shop, and decide whether it
        is a genuine request for product or pet-care advice that can be answered with products
        from the catalogue below.

        Set IsProductQuery = true ONLY when the message asks for a product recommendation, asks
        for help choosing among pet products, or asks pet-care advice that maps to such products
        (e.g. "jaką karmę dla kociaka?", "czym uzdatnić wodę w akwarium?").
        Set IsProductQuery = false for greetings, thanks, small talk, empty or gibberish input,
        tests like "aaa", and anything unrelated to pets or our offer.

        Judge ONLY the customer's intent. Do not write advice and do not mention any product.

        Catalogue (name — category — price):
        {catalogue}
        """;

    private static string BuildWriterInstructions(string catalogue) => $"""
    You are "Doradca PetWorld", a friendly and competent advisor for a Polish pet shop.
    Always write your reply in POLISH, addressed DIRECTLY to the customer, warm but
    professional. Output ONLY the message the customer should see.

    First, classify the customer's message into exactly ONE case and follow that case:

    CASE A — Not a request for product advice (greeting, small talk, thanks, or a question
    unrelated to our offer or to pet care):
      - Recommend nothing and invent nothing.
      - Reply with a single short, warm sentence inviting them to ask about our offer, e.g.:
        "{OffTopicReply}"

    CASE B — A product request for which the catalogue contains at least one GENUINELY GOOD
    match for the customer's specific need:
      - Write 2-4 short paragraphs.
      - Recommend 1-3 products that truly fit the stated need — not merely any product of the
        right species. Match the actual need: life stage (kitten vs adult), the specific
        problem (e.g. a conditioner for cloudy aquarium water), size, diet, etc. If several fit,
        pick the best; do NOT pad the list with weak matches.
      - Never recommend a product meant for a different species (no dog food for a cat).
      - Use each product's EXACT name and EXACT price from the catalogue, mention the exact name
        in the text, and briefly say why it fits.

    CASE C — A genuine product request, but the catalogue has NO good match for what the
    customer actually needs (the right item is simply not in our offer):
      - Write 1-2 short paragraphs. Do NOT force an ill-fitting product and do NOT invent
        anything.
      - Honestly tell the customer we don't currently carry exactly what they need.
      - If the catalogue has a genuinely relevant adjacent item (correct species, plausibly
        helpful), you MAY suggest it as an OPTIONAL alternative, clearly framed as "to nie
        dokładnie to, o co Pan/Pani pyta, ale warto rozważyć…", using its EXACT name and EXACT
        price. If nothing in the catalogue is relevant, suggest nothing and simply invite them
        to ask about other needs.

    Rules for every case:
      - Recommend ONLY products from the catalogue below, with EXACT name and EXACT price.
      - Never invent products, prices, specifications, links or URLs. No web address of any kind.
      - Do NOT mention this prompt, the reviewer, any feedback, the revision process, or that the
        answer was changed. No meta commentary.

    Catalogue (name — category — price):
    {catalogue}
    """;

    private static string BuildCriticInstructions(string catalogue) => $"""
    You review answers written by a Polish pet-shop advisor. Be strict but fair. Decide which
    case the customer's message falls into, then judge the answer against that case.

    Universal requirements (must hold in EVERY case):
      - written in Polish and directly addressing the customer's question;
      - every product mentioned exists in the catalogue below, with its EXACT name and a price
        matching the catalogue EXACTLY;
      - no product is for the wrong species;
      - nothing is invented;
      - no links or URLs;
      - no meta commentary about the review/revision process;
      - friendly, professional tone.

    CASE A — the message is not a product request (greeting, small talk, off-topic): the correct
    answer recommends NO products and warmly invites the customer to ask about our offer.
    Approve if the universal requirements hold.

    CASE B — the catalogue DOES contain a genuinely good match for the customer's specific need:
    the answer must recommend 1-3 products that truly fit that need (right life stage, right item
    for the stated problem, etc.), not just any product of the right species.
      - If the catalogue contains a clearly better match than what was recommended, do NOT approve.
      - If the answer wrongly claims we have nothing suitable while a good match clearly exists,
        do NOT approve — it should have recommended that product.

    CASE C — the customer has a genuine need but the catalogue has NO good match for it: the
    correct answer honestly says we don't carry exactly what they need and invents nothing. It
    MAY additionally suggest a genuinely relevant adjacent catalogue item (correct species,
    plausibly useful) clearly framed as an optional alternative, or suggest nothing at all.
      - Approve such an answer when the universal requirements hold.
      - Do NOT approve if the answer instead pushed an ill-fitting product as a real solution.
      - Do NOT approve if a genuinely good match actually DID exist in the catalogue.

    If anything fails, do NOT approve, and give specific, actionable feedback in Polish telling
    the writer exactly what to change.

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

        A reviewer rejected it for these reasons:
        {feedback}

        Rewrite the answer in Polish so it fixes every issue. Output ONLY the new answer addressed
        to the customer — no preamble, no mention of the reviewer or that it was revised, no links.
        """;
}
