using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Application.Services;
using MeuProjetoFinanceiro.Core.Enums;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Services;

public class DashboardFinanceiroConsolidadoService : IDashboardFinanceiroService
{
    private readonly AppDbContext _context;
    private readonly ICrudService<OrcamentoDto> _orcamentos;

    public DashboardFinanceiroConsolidadoService(AppDbContext context, ICrudService<OrcamentoDto> orcamentos)
    {
        _context = context;
        _orcamentos = orcamentos;
    }

    public async Task<DashboardFinanceiroDto> GetResumoAsync(DateTime? inicio = null, DateTime? fim = null)
    {
        var dataInicio = inicio?.Date ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var dataFim = fim?.Date ?? dataInicio.AddMonths(1).AddDays(-1);
        var meses = EnumerarMeses(dataInicio, dataFim).ToList();
        var anos = meses.Select(m => m.Year).Distinct().ToList();

        if (_context.Database.IsSqlite())
        {
            await GarantirReceitasPadraoAsync(dataInicio.Year);
        }

        var receitasBase = await _context.ReceitasMensais
            .Where(r => anos.Contains(r.Ano))
            .AsNoTracking()
            .ToListAsync();

        var faturas = await _context.FaturasCartaoCredito
            .Where(f => anos.Contains(f.Ano))
            .AsNoTracking()
            .ToListAsync();

        var receitaTotal = meses.Sum(mes =>
            receitasBase.Where(r => r.Ano == mes.Year && r.Mes == mes.Month).Sum(r => r.Valor));

        var faturasCartao = meses.Sum(mes =>
            faturas.Where(f => f.Ano == mes.Year && f.Mes == mes.Month).Sum(f => f.ValorTotal));

        if (_context.Database.IsNpgsql())
        {
            return CriarResumoLeve(dataInicio, receitaTotal, faturasCartao);
        }

        var transacoes = await _context.Transacoes
            .Where(t => t.Data.Date >= dataInicio && t.Data.Date <= dataFim)
            .AsNoTracking()
            .Include(t => t.Categoria)
            .Include(t => t.ContaFinanceira)
            .ToListAsync();

        var despesasFixas = meses.Sum(mes => transacoes
            .Where(t => t.Data.Year == mes.Year && t.Data.Month == mes.Month && PlanejamentoDespesasFixasService.EhDespesaFixa(t.Descricao))
            .Sum(t => t.Valor));

        faturasCartao = meses.Sum(mes =>
        {
            var faturaFechada = faturas.Where(f => f.Ano == mes.Year && f.Mes == mes.Month).Sum(f => f.ValorTotal);
            if (faturaFechada > 0)
            {
                return faturaFechada;
            }

            return transacoes
                .Where(t => t.Data.Year == mes.Year && t.Data.Month == mes.Month && PareceCartao(t.Descricao))
                .Sum(t => t.Valor);
        });

        var outrasDespesas = meses.Sum(mes => transacoes
            .Where(t => t.Data.Year == mes.Year
                        && t.Data.Month == mes.Month
                        && t.Tipo == TipoTransacao.Despesa
                        && !PlanejamentoDespesasFixasService.EhDespesaFixa(t.Descricao)
                        && !PareceCartao(t.Descricao))
            .Sum(t => t.Valor));

        var despesaTotal = despesasFixas + faturasCartao + outrasDespesas;

        var resumoPeriodo = new List<TransacaoDto>
        {
            new()
            {
                Data = dataInicio,
                Descricao = "Receitas do periodo",
                CategoriaNome = "Entradas",
                Tipo = TipoTransacao.Receita,
                Valor = receitaTotal
            },
            new()
            {
                Data = dataInicio,
                Descricao = "Despesas fixas",
                CategoriaNome = "Fixas",
                Tipo = TipoTransacao.Despesa,
                Valor = despesasFixas
            },
            new()
            {
                Data = dataInicio,
                Descricao = "Cartao de credito",
                CategoriaNome = "Cartao",
                Tipo = TipoTransacao.Despesa,
                Valor = faturasCartao
            },
            new()
            {
                Data = dataInicio,
                Descricao = "Outras despesas",
                CategoriaNome = "Variaveis",
                Tipo = TipoTransacao.Despesa,
                Valor = outrasDespesas
            }
        };

        return new DashboardFinanceiroDto
        {
            ReceitaTotal = receitaTotal,
            DespesaTotal = despesaTotal,
            Resultado = receitaTotal - despesaTotal,
            SaldoTotal = receitaTotal - despesaTotal,
            TransacoesPendentes = transacoes.Count(t => !t.Efetivada),
            UltimasTransacoes = resumoPeriodo,
            Orcamentos = (await _orcamentos.GetAllAsync())
                .Where(o => o.Mes == dataInicio.Month && o.Ano == dataInicio.Year)
                .ToList(),
            LabelsCategorias = ["Despesas fixas", "Cartao", "Outras despesas"],
            ValoresCategorias = [despesasFixas, faturasCartao, outrasDespesas]
        };
    }

    private static DashboardFinanceiroDto CriarResumoLeve(DateTime dataInicio, decimal receitaTotal, decimal faturasCartao)
    {
        var despesaTotal = faturasCartao;

        return new DashboardFinanceiroDto
        {
            ReceitaTotal = receitaTotal,
            DespesaTotal = despesaTotal,
            Resultado = receitaTotal - despesaTotal,
            SaldoTotal = receitaTotal - despesaTotal,
            TransacoesPendentes = 0,
            UltimasTransacoes =
            [
                new()
                {
                    Data = dataInicio,
                    Descricao = "Receitas do periodo",
                    CategoriaNome = "Entradas",
                    Tipo = TipoTransacao.Receita,
                    Valor = receitaTotal
                },
                new()
                {
                    Data = dataInicio,
                    Descricao = "Cartao de credito",
                    CategoriaNome = "Cartao",
                    Tipo = TipoTransacao.Despesa,
                    Valor = faturasCartao
                }
            ],
            Orcamentos = [],
            LabelsCategorias = ["Cartao"],
            ValoresCategorias = [faturasCartao]
        };
    }

    private async Task GarantirReceitasPadraoAsync(int ano)
    {
        if (await _context.ReceitasMensais.AnyAsync(r => r.Ano == ano))
        {
            return;
        }

        var receitas = new List<Core.Entities.ReceitaMensal>();
        for (var mes = 1; mes <= 12; mes++)
        {
            receitas.Add(new Core.Entities.ReceitaMensal { Ano = ano, Mes = mes, Descricao = "SALARIO LIQUIDO TCS", Valor = 7586.34m });
            receitas.Add(new Core.Entities.ReceitaMensal { Ano = ano, Mes = mes, Descricao = "SALARIO LIQUIDO OTICAS BRASIL", Valor = mes == 1 ? 1250m : 5125m });
        }

        receitas.Add(new Core.Entities.ReceitaMensal { Ano = ano, Mes = 7, Descricao = "13 SALARIO LIQUIDO / FERIAS", Valor = 4500m });
        _context.ReceitasMensais.AddRange(receitas);
        await _context.SaveChangesAsync();
    }

    private static IEnumerable<DateTime> EnumerarMeses(DateTime inicio, DateTime fim)
    {
        var cursor = new DateTime(inicio.Year, inicio.Month, 1);
        var limite = new DateTime(fim.Year, fim.Month, 1);

        while (cursor <= limite)
        {
            yield return cursor;
            cursor = cursor.AddMonths(1);
        }
    }

    private static bool PareceCartao(string descricao)
    {
        var texto = descricao.ToUpperInvariant();
        return texto.Contains("FATURAS")
               || texto.Contains("CARTAO")
               || texto.Contains("CARTÃO")
               || texto.Contains("MAE PASSAGEM")
               || texto.Contains("DENTISTA")
               || texto.Contains("DESPESA CART");
    }
}
