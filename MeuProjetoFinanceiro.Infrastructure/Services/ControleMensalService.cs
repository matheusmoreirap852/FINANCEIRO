using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Enums;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Services;

public class ControleMensalService : IControleMensalService
{
    private readonly AppDbContext _context;

    public ControleMensalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ControleMensalDto> ObterAsync(int ano, CancellationToken cancellationToken = default)
    {
        await GarantirReceitasPadraoAsync(ano, cancellationToken);

        var receitas = await _context.ReceitasMensais
            .Where(r => r.Ano == ano)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var despesas = await _context.Transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa && t.Data.Year == ano)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var faturas = await _context.FaturasCartaoCredito
            .Where(f => f.Ano == ano)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var meses = Enumerable.Range(1, 12).ToList();
        var linhas = new List<ControleMensalLinhaDto>();

        foreach (var grupo in receitas.GroupBy(r => r.Descricao).OrderBy(g => g.Key))
        {
            linhas.Add(new ControleMensalLinhaDto
            {
                Grupo = "Entradas",
                Nome = grupo.Key,
                Valores = ValoresPorMes(meses, mes => grupo.Where(r => r.Mes == mes).Sum(r => r.Valor))
            });
        }

        var totalEntradas = meses.Select(mes => receitas.Where(r => r.Mes == mes).Sum(r => r.Valor)).ToList();
        linhas.Add(new ControleMensalLinhaDto
        {
            Grupo = "Entradas",
            Nome = "Total Rendimentos Variaveis",
            Valores = totalEntradas.Select(v => (decimal?)v).ToList(),
            Destaque = true
        });

        var despesasFixas = despesas.Where(d => PlanejamentoDespesasFixasService.EhDespesaFixa(d.Descricao)).ToList();
        foreach (var grupo in despesasFixas.GroupBy(d => NormalizarDescricao(d.Descricao)).OrderBy(g => g.Key))
        {
            linhas.Add(new ControleMensalLinhaDto
            {
                Grupo = "Despesas Fixas",
                Nome = grupo.Key,
                Valores = ValoresPorMes(meses, mes => grupo.Where(d => d.Data.Month == mes).Sum(d => d.Valor))
            });
        }

        var totalFixas = meses.Select(mes => despesasFixas.Where(d => d.Data.Month == mes).Sum(d => d.Valor)).ToList();
        linhas.Add(new ControleMensalLinhaDto
        {
            Grupo = "Despesas Fixas",
            Nome = "DESPESAS FIXAS",
            Valores = totalFixas.Select(v => (decimal?)v).ToList(),
            Destaque = true
        });

        var totalCartao = meses.Select(mes => faturas.Where(f => f.Mes == mes).Sum(f => f.ValorTotal)).ToList();
        linhas.Add(new ControleMensalLinhaDto
        {
            Grupo = "Cartao",
            Nome = "ITAU",
            Valores = totalCartao.Select(v => (decimal?)v).ToList(),
            Destaque = true,
            NegativoRuim = true
        });

        var outros = despesas
            .Where(d => !PlanejamentoDespesasFixasService.EhDespesaFixa(d.Descricao) && !PareceCartao(d.Descricao))
            .ToList();
        var totalOutros = meses.Select(mes => outros.Where(d => d.Data.Month == mes).Sum(d => d.Valor)).ToList();

        var totalDespesas = meses.Select(mes => totalFixas[mes - 1] + totalCartao[mes - 1] + totalOutros[mes - 1]).ToList();
        linhas.Add(new ControleMensalLinhaDto
        {
            Grupo = "Resumo",
            Nome = "TOTAL DE DESPESAS",
            Valores = totalDespesas.Select(v => (decimal?)v).ToList(),
            Destaque = true,
            NegativoRuim = true
        });

        linhas.Add(new ControleMensalLinhaDto
        {
            Grupo = "Resumo",
            Nome = "VALOR A PAGAR",
            Valores = totalDespesas.Select(v => (decimal?)v).ToList(),
            Destaque = true,
            NegativoRuim = true
        });

        linhas.Add(new ControleMensalLinhaDto
        {
            Grupo = "Resumo",
            Nome = "SALDO FINAL",
            Valores = meses.Select(mes => (decimal?)(totalEntradas[mes - 1] - totalDespesas[mes - 1])).ToList(),
            Destaque = true
        });

        return new ControleMensalDto
        {
            Ano = ano,
            Meses = meses.Select(mes => new DateTime(ano, mes, 1).ToString("MMM-yy")).ToList(),
            Linhas = linhas
        };
    }

    private async Task GarantirReceitasPadraoAsync(int ano, CancellationToken cancellationToken)
    {
        if (await _context.ReceitasMensais.AnyAsync(r => r.Ano == ano, cancellationToken))
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
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<decimal?> ValoresPorMes(IEnumerable<int> meses, Func<int, decimal> valorFactory)
        => meses.Select(mes =>
        {
            var valor = valorFactory(mes);
            return valor == 0 ? null : (decimal?)valor;
        }).ToList();

    private static string NormalizarDescricao(string descricao)
    {
        var separador = descricao.LastIndexOf(" - ", StringComparison.Ordinal);
        var nome = separador > 0 ? descricao[..separador].Trim() : descricao.Trim();
        return nome.Equals("Uber", StringComparison.OrdinalIgnoreCase) ? "Parcela da moto" : nome;
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
