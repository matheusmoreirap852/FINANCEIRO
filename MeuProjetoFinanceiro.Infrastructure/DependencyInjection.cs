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

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICrudService<ContaFinanceiraDto>, ContaFinanceiraService>();
        services.AddScoped<ICrudService<CategoriaDto>, CategoriaService>();
        services.AddScoped<ICrudService<TransacaoDto>, TransacaoService>();
        services.AddScoped<ICrudService<OrcamentoDto>, OrcamentoService>();
        services.AddScoped<IDashboardFinanceiroService, DashboardFinanceiroService>();
        services.AddScoped<IImportacaoPlanilhaFinanceiraService, ImportacaoPlanilhaFinanceiraService>();
        services.AddScoped<IPlanejamentoCartaoCreditoService, PlanejamentoCartaoCreditoService>();

        return services;
    }
}
