using System.ClientModel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;
using PetWorld.Application.Abstractions;
using PetWorld.Infrastructure.Ai;
using PetWorld.Infrastructure.Cart;
using PetWorld.Infrastructure.Identity;
using PetWorld.Infrastructure.Persistence;

namespace PetWorld.Infrastructure;

/// <summary>
/// The single composition point for the outer ring. Web's Program.cs calls this and
/// nothing else from Infrastructure — keeping EF, MAF and Identity out of the UI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        AddDatabase(services, config);
        AddRepositoriesAndCart(services);
        AddAiAdvisor(services, config);
        AddIdentityAndAuth(services);
        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        // Explicit server version so DI does not need a live DB connection at startup
        // (the actual readiness wait happens in the migration retry loop).
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        services.AddDbContext<PetWorldDbContext>(options =>
            options.UseMySql(connectionString, serverVersion,
                mySql => mySql.EnableRetryOnFailure()));
    }

    private static void AddRepositoriesAndCart(IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
        services.AddScoped<ICartService, CartService>(); // in-memory, per Blazor circuit
    }

    private static void AddAiAdvisor(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IChatClient>(sp => CreateChatClient(sp, config));
        services.AddScoped<IProductAdvisorService, ProductAdvisorService>();
    }

    private static IChatClient CreateChatClient(IServiceProvider sp, IConfiguration config)
    {
        var apiKey = config["OPENAI_API_KEY"];
        // The model is configured here ONLY (Ai:Model / env AI__MODEL), never hardcoded elsewhere.
        var model = config["Ai:Model"] ?? "gpt-4o-mini";

        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("PetWorld.Advisor");

        // Optional OpenAI-compatible base URL so the advisor can target a LOCAL LLM server
        // (LM Studio, Ollama, llama.cpp, vLLM, ...). Must be an absolute http(s) URL with the
        // /v1 suffix, e.g. http://localhost:1234/v1. A malformed value is ignored (never throws,
        // so a typo cannot crash the app) — we just warn and fall back.
        var rawBaseUrl = config["Ai:BaseUrl"] ?? config["OPENAI_BASE_URL"];
        Uri? endpoint = null;
        if (!string.IsNullOrWhiteSpace(rawBaseUrl))
        {
            if (Uri.TryCreate(rawBaseUrl.Trim(), UriKind.Absolute, out var parsed) &&
                (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
            {
                endpoint = parsed;
            }
            else
            {
                logger.LogWarning(
                    "Ignoring invalid AI base URL '{BaseUrl}'. Expected an absolute URL like http://host:port/v1.",
                    rawBaseUrl);
            }
        }

        // Local OpenAI-compatible servers usually ignore the key — allow a placeholder so
        // they still work when only a base URL (and no real key) is configured.
        if (endpoint is not null && string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = "local";
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("OPENAI_API_KEY is not set — the AI advisor will be unavailable until it is configured.");
            return new NotConfiguredChatClient();
        }

        var options = new OpenAIClientOptions();
        if (endpoint is not null)
        {
            options.Endpoint = endpoint;
            logger.LogInformation("Advisor using OpenAI-compatible endpoint {Endpoint} (model {Model}).", endpoint, model);
        }

        return new OpenAIClient(new ApiKeyCredential(apiKey), options)
            .GetChatClient(model)
            .AsIChatClient();
    }

    private static void AddIdentityAndAuth(IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
        }).AddIdentityCookies();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<PetWorldDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

        // SignInManager writes the auth cookie via the current HttpContext.
        services.AddHttpContextAccessor();

        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        services.AddScoped<IAuthService, IdentityAuthService>();
    }
}
