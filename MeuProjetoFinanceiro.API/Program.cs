using MeuProjetoFinanceiro.Infrastructure;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var culturaBrasil = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBrasil;
CultureInfo.DefaultThreadCurrentUICulture = culturaBrasil;

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Auth:JwtKey"]
            ?? "dev-only-change-this-key-with-at-least-32-characters";
        var issuer = builder.Configuration["Auth:Issuer"] ?? "MeuProjetoFinanceiro";
        var audience = builder.Configuration["Auth:Audience"] ?? "MeuProjetoFinanceiro.App";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2),
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
