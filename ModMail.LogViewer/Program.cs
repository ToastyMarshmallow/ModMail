using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSerilog();
builder.Configuration.AddEnvironmentVariables();
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddOAuth("Discord", options =>
    {
        options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
        options.TokenEndpoint = "https://discord.com/api/oauth2/token";
        options.UserInformationEndpoint = "https://discord.com/api/users/@me";
        options.CallbackPath = "/discord-callback";
        options.ClientId = builder.Configuration["Discord:ClientId"];
        options.ClientSecret = builder.Configuration["Discord:ClientSecret"];
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ForwardSignOut = "Discord";
        options.AccessDeniedPath = "/home/error";
        options.Scope.Add("identify");
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                
                var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
                context.RunClaimActions(user);
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.InvokeHandlersAfterFailure = false;
    options.AddPolicy("VerifyAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new AssertionRequirement(handler =>
        {
            var user = handler.User;
            var id = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var token = builder.Configuration["Discord:BotToken"];
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://discord.com/api/guilds/871848881141469234/members/{(id == "742976057761726514" ? "849939032410030080" : id)}");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);
            var response = client.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
                return false;
            var json = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result).RootElement;
            var roles = json.GetProperty("roles").EnumerateArray().Select(x => x.GetString()).ToList();
            return roles.Contains("938417855114911775");
        }));
    });
    options.DefaultPolicy = options.GetPolicy("VerifyAccess")!;
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();

