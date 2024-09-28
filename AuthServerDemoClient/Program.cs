using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOAuth("custom", o =>
    {
        o.SignInScheme = "cookie";
        o.ClientId = "x";
        o.ClientSecret = "y";

        o.AuthorizationEndpoint = "https://localhost:7145/oauth/authorize";
        o.TokenEndpoint = "https://localhost:7145/oauth/token";
        o.CallbackPath = "/oauth/custom-cb";

        o.UsePkce = true;
        o.ClaimActions.MapJsonKey("sub", "sub");
        //get data after getting autenticated
        o.Events.OnCreatingTicket = async ctx =>
        {
            //map claims
            var payloadJwt = ctx.AccessToken.Split(".")[1]; //here we take the paylod portion of jwt
            var payloadJson = Base64UrlTextEncoder.Decode(payloadJwt);
            var payload = JsonDocument.Parse(payloadJson);
            ctx.RunClaimActions(payload.RootElement);
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();