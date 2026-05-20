using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Application.Services;

public class DashboardFinanceiroService : IDashboardFinanceiroService
{
    private readonly IRepository<ContaFinanceira> _contas;
    private readonly IRepository<Transacao> _transacoes;
    private readonly ICrudService<OrcamentoDto> _orcamentos;

    public DashboardFinanceiroService(
        IRepository<ContaFinanceira> contas,
        IRepository<Transacao> transacoes,
        ICrudService<OrcamentoDto> orcamentos)
    {
        _contas = contas;
        _transacoes = transacoes;
        _orcamentos = orcamentos;
    }

    public async Task<DashboardFinanceiroDto> GetResumoAsync(DateTime? inicio = null, DateTime? fim = null)
    {
        var dataInicio = inicio?.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var dataFim = fim?.Date ?? dataInicio.AddMonths(1).AddDays(-1);

        var contas = await _contas.GetAllAsync();
        var transacoes = (await _transacoes.GetAllAsync())
            .Where(t => t.Data.Date >= dataInicio && t.Data.Date <= dataFim)
            .ToList();

        var receitas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita && t.Efetivada).Sum(t => t.Valor);
        var despesas = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa && t.Efetivada).Sum(t => t.Valor);
        var movimentacaoTotal = (await _transacoes.GetAllAsync())
            .Where(t => t.Efetivada)
            .Sum(t => t.Tipo == TipoTransacao.Despesa ? -t.Valor : t.Valor);

        var despesasPorCategoria = transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa)
            .GroupBy(t => t.Categoria?.Nome ?? "Sem categoria")
            .OrderByDescending(g => g.Sum(t => t.Valor))
            .Take(6)
            .ToList();

        return new DashboardFinanceiroDto
        {
            ReceitaTotal = receitas,
            DespesaTotal = despesas,
            Resultado = receitas - despesas,
            SaldoTotal = contas.Sum(c => c.SaldoInicial) + movimentacaoTotal,
            TransacoesPendentes = transacoes.Count(t => !t.Efetivada),
            UltimasTransacoes = transacoes.OrderByDescending(t => t.Data).Take(8).Select(TransacaoService.ToDto).ToList(),
            Orcamentos = (await _orcamentos.GetAllAsync()).Where(o => o.Mes == dataInicio.Month && o.Ano == dataInicio.Year).ToList(),
            LabelsCategorias = despesasPorCategoria.Select(g => g.Key).ToList(),
            ValoresCategorias = despesasPorCategoria.Select(g => g.Sum(t => t.Valor)).ToList()
        };
    }
}
