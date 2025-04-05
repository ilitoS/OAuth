using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OAuth.Server.ExternalModels;
using OAuth.Server.Models;
using OAuth.Server.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddDbContext<OauthAppContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MasterDb"),
    o => o.SetPostgresVersion(17, 4)));

var userService = builder.Services.BuildServiceProvider().GetService<IUserService>();

builder.Services.AddControllers();
builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    })
    .AddTwitter("twitter", t =>
    {
        t.ClientId = builder.Configuration.GetSection("OAuth:Twitter:ClientId").Value;
        t.ClientSecret = builder.Configuration.GetSection("OAuth:Twitter:ClientSecret").Value;
        t.SaveTokens = true;

        t.Scope.Add("users.read");
        t.Scope.Add("tweet.read");

        t.Events.OnCreatingTicket = async context =>
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await client.GetAsync("https://api.twitter.com/2/users/me");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("data");

                if (userService.GetUserByUsername(userData.GetString("username"), "twitter") == null)
                    await userService.SaveUserAsync(userData.GetString("email"), userData.GetString("username"), context.AccessToken, string.Empty, "twitter", null);

                var identity = (ClaimsIdentity)context.Principal.Identity;
                identity.AddClaim(new Claim("Username", userData.GetString("username")));
                identity.AddClaim(new Claim("LoginProvider", "twitter"));
            }

        };
    })
    .AddGoogleOpenIdConnect("google", g =>
    {
        g.ClientId = builder.Configuration.GetSection("OAuth:Google:ClientId").Value;
        g.ClientSecret = builder.Configuration.GetSection("OAuth:Google:ClientSecret").Value;
        g.SaveTokens = true;
        g.Scope.Add("https://www.googleapis.com/auth/userinfo.profile");

        g.Events.OnTicketReceived = async context =>
        {
            var email = context.Principal.Claims.FirstOrDefault(x => x.Type.Contains("emailaddress")).Value;
            var identity = (ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new Claim("Email", email));
            identity.AddClaim(new Claim("LoginProvider", "google"));
            
            await Task.FromResult(0);
        };

        g.Events.OnTokenResponseReceived = async context =>
        {
            string accessToken = context.TokenEndpointResponse.AccessToken;
            string idToken = context.TokenEndpointResponse.IdToken;    
            int expiresIn = int.Parse(context.TokenEndpointResponse.ExpiresIn);
            string refreshToken = context.TokenEndpointResponse.RefreshToken;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("https://www.googleapis.com/oauth2/v1/userinfo");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var email = JsonSerializer.Deserialize<JsonElement>(json).GetString("email");
                var expiryDate = DateTime.UtcNow.AddSeconds(expiresIn);

                if (userService.GetUserByEmail(email, "google") == null)
                    await userService.SaveUserAsync(email, string.Empty, accessToken, refreshToken, "google", expiryDate);
            }
        };
    })
    .AddOAuth("github", gh =>
    {
        gh.ClientId = builder.Configuration.GetSection("OAuth:GitHub:ClientId").Value;
        gh.ClientSecret = builder.Configuration.GetSection("OAuth:GitHub:ClientSecret").Value;
        gh.AuthorizationEndpoint = builder.Configuration.GetSection("OAuth:GitHub:AuthorizationEndpoint").Value;
        gh.TokenEndpoint = builder.Configuration.GetSection("OAuth:GitHub:TokenEndpoint").Value;
        gh.UserInformationEndpoint = builder.Configuration.GetSection("OAuth:GitHub:UserInformationEndpoint").Value;
        gh.CallbackPath = builder.Configuration.GetSection("OAuth:GitHub:CallbackPath").Value;
        gh.SaveTokens = true;

        gh.Scope.Add("read:user");
        gh.Scope.Add("user:email");

        gh.Events.OnCreatingTicket = async context =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            using var response = await context.Backchannel.SendAsync(request);
            var userJson = await response.Content.ReadFromJsonAsync<JsonElement>();
            var user = JsonSerializer.Deserialize<GithubUser>(userJson);

            using var requestEmail = new HttpRequestMessage(HttpMethod.Get, builder.Configuration.GetSection("OAuth:GitHub:UserEmailEndpoint").Value);
            requestEmail.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            using var responseEmail = await context.Backchannel.SendAsync(requestEmail);
            var emails = await responseEmail.Content.ReadFromJsonAsync<List<GithubEmail>>();
            string email = emails?.FirstOrDefault(x => x.Primary)?.Email ?? string.Empty;

            if (userService.GetUserByUsername(user.Username, "github") == null)
                await userService.SaveUserAsync(email, user.Username, context.AccessToken, null, "github", null);

            var identity = (ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new Claim("Email", email));
            identity.AddClaim(new Claim("LoginProvider", "github"));
        };

    });
builder.Services.AddAuthorization();

var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();