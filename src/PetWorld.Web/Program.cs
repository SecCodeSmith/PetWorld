using PetWorld.Application.Abstractions;
using PetWorld.Infrastructure;
using PetWorld.Infrastructure.Persistence;
using PetWorld.Web.Components;
using PetWorld.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server (interactive server). Interactivity is opted-in per page, so the
// login/register pages can stay static-SSR and set the auth cookie during a form POST.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth state cascades to all components; pages opt in with [Authorize] / <AuthorizeView>.
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

// Per-circuit advisor conversation, so the chat survives navigation and can be reopened.
builder.Services.AddScoped<ChatSessionState>();

// The outer ring: EF Core + MySQL, ASP.NET Core Identity, the MAF advisor and the cart.
// This is the ONLY call into Infrastructure from the web project.
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Apply migrations + seed the catalogue and demo user before serving traffic.
// The migration step retries internally so the app does not crash-loop on first boot.
await DbInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout is a real HTTP POST (cookie must be cleared in a request context), routed
// through IAuthService so no Identity type appears in the web project.
app.MapPost("/account/logout", async (IAuthService auth) =>
{
    await auth.LogoutAsync();
    return Results.LocalRedirect("~/");
});

app.Run();
