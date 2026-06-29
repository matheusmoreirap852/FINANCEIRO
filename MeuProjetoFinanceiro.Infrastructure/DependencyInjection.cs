using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Application.Services;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using MeuProjetoFinanceiro.Infrastructure.Repositories;
using MeuProjetoFinanceiro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeuProjetoFinanceiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=financeiro.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (EhPostgres(connectionString))
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                options.UseNpgsql(connectionString);
                return;
            }

            options.UseSqlite(connectionString);
        });
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICrudService<ContaFinanceiraDto>, ContaFinanceiraService>();
        services.AddScoped<ICrudService<CategoriaDto>, CategoriaService>();
        services.AddScoped<ICrudService<TransacaoDto>, TransacaoService>();
        services.AddScoped<ICrudService<OrcamentoDto>, OrcamentoService>();
        services.AddScoped<IDashboardFinanceiroService, DashboardFinanceiroConsolidadoService>();
        services.AddScoped<IImportacaoPlanilhaFinanceiraService, ImportacaoPlanilhaFinanceiraService>();
        services.AddScoped<IPlanejamentoCartaoCreditoService, PlanejamentoCartaoCreditoService>();
        services.AddScoped<IPlanejamentoDespesasFixasService, PlanejamentoDespesasFixasService>();
        services.AddScoped<IControleMensalService, ControleMensalService>();

        return services;
    }

    private static bool EhPostgres(string connectionString)
        => connectionString.StartsWith("Host=", StringComparison.OrdinalIgnoreCase)
           || connectionString.StartsWith("Server=", StringComparison.OrdinalIgnoreCase)
           || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
           || connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase);
}
