using MeuProjetoFinanceiro.Infrastructure;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var culturaBrasil = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaBrasil;
CultureInfo.DefaultThreadCurrentUICulture = culturaBrasil;

builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
