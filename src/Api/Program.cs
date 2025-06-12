using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
    // Configure how the Client app is authenticating with the backend (this app)
    {
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
    options.DefaultChallengeScheme = "GitHub";
    })
    .AddCookie("Cookies")
    // Configure how to do the OAuth flow
    .AddOAuth("GitHub", options =>
    {
    options.ClientId = builder.Configuration["GitHub:ClientId"];
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"];
    options.CallbackPath = "/signin-github";

    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";

    options.Scope.Add("user:email");

    // Enrich client credentials (cookie) with information
    options.ClaimActions.MapJsonKey("urn:github:login", "login");
    options.ClaimActions.MapJsonKey("urn:github:id", "id");
    options.ClaimActions.MapJsonKey("urn:github:url", "html_url");

    options.SaveTokens = true;

    // Do the proof key exchange
    options.Events.OnCreatingTicket = async context =>
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Accept.Add(new("application/json"));
        request.Headers.Authorization = new("Bearer", context.AccessToken);

        var response = await context.Backchannel.SendAsync(request);
        var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        context.RunClaimActions(user.RootElement);
    };
    });
    
// Enable Authorization
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Setup OAuth endpoints
app.MapGet("/github-login", async context =>
{
  if (!context.User.Identity?.IsAuthenticated ?? true)
    await context.ChallengeAsync("GitHub");

  else
    context.Response.Redirect("/user");
});

app.Run();
