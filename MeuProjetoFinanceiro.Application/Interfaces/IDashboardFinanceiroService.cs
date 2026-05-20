using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Application.Interfaces;

public interface IDashboardFinanceiroService
{
    Task<DashboardFinanceiroDto> GetResumoAsync(DateTime? inicio = null, DateTime? fim = null);
}
