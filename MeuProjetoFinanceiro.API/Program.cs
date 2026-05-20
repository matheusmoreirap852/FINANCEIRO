using MeuProjetoFinanceiro.Infrastructure;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var culturaBrasil = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBrasil;
CultureInfo.DefaultThreadCurrentUICulture = culturaBrasil;

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializer>();
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
app.MapControllers();

app.Run();
