using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Services;

public class PlanejamentoCartaoCreditoService : IPlanejamentoCartaoCreditoService
{
    private readonly AppDbContext _context;

    public PlanejamentoCartaoCreditoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlanejamentoCartaoCreditoDto> ObterAsync(int ano, CancellationToken cancellationToken = default)
    {
        await GarantirReceitasPadraoAsync(ano, cancellationToken);

        var receitas = await _context.ReceitasMensais
            .Where(r => r.Ano == ano)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lancamentos = await _context.LancamentosCartaoCredito
            .AsNoTracking()
            .OrderByDescending(l => l.AnoInicial)
            .ThenByDescending(l => l.MesInicial)
            .ToListAsync(cancellationToken);

        var faturas = await _context.FaturasCartaoCredito
            .Where(f => f.Ano == ano)
            .AsNoTracking()
            .OrderBy(f => f.Ano)
            .ThenBy(f => f.Mes)
            .ToListAsync(cancellationToken);

        var despesasImportadas = await _context.Transacoes
            .Where(t => t.Data.Year == ano && t.Tipo == TipoTransacao.Despesa)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var meses = Enumerable.Range(1, 12)
            .Select(mes =>
            {
                var receitaMes = receitas.Where(r => r.Mes == mes).Sum(r => r.Valor);
                var faturaProjetada = lancamentos.Sum(l => ValorParcelaNoMes(l, ano, mes));
                var faturaFechada = faturas.Where(f => f.Mes == mes).Sum(f => f.ValorTotal);
                var despesasFixas = despesasImportadas
                    .Where(t => t.Data.Year == ano && t.Data.Month == mes && PlanejamentoDespesasFixasService.EhDespesaFixa(t.Descricao))
                    .Sum(t => t.Valor);
                var outrasDespesas = despesasImportadas
                    .Where(t => t.Data.Year == ano && t.Data.Month == mes && !PareceCartao(t.Descricao) && !PlanejamentoDespesasFixasService.EhDespesaFixa(t.Descricao))
                    .Sum(t => t.Valor);

                return new ResumoCartaoMesDto
                {
                    Ano = ano,
                    Mes = mes,
                    Periodo = new DateTime(ano, mes, 1).ToString("MMM/yyyy"),
                    ReceitaPrevista = receitaMes,
                    FaturaProjetada = faturaFechada > 0 ? faturaFechada : faturaProjetada + despesasImportadas
                        .Where(t => t.Data.Year == ano && t.Data.Month == mes && PareceCartao(t.Descricao))
                        .Sum(t => t.Valor),
                    DespesasFixas = despesasFixas,
                    OutrasDespesas = outrasDespesas
                };
            })
            .ToList();

        return new PlanejamentoCartaoCreditoDto
        {
            Ano = ano,
            ReceitaMedia = meses.Average(m => m.ReceitaPrevista),
            MaiorFaturaProjetada = meses.Max(m => m.FaturaProjetada),
            MesMaisApertado = meses.Min(m => m.SaldoFinalProjetado),
            Meses = meses,
            Lancamentos = lancamentos.Select(ToDto).ToList(),
            Faturas = faturas.Select(ToDto).ToList()
        };
    }

    public async Task CriarLancamentoAsync(LancamentoCartaoCreditoDto dto, CancellationToken cancellationToken = default)
    {
        _context.LancamentosCartaoCredito.Add(new LancamentoCartaoCredito
        {
            Descricao = dto.Descricao,
            Cartao = string.IsNullOrWhiteSpace(dto.Cartao) ? "Cartao geral" : dto.Cartao,
            ValorTotal = dto.ValorTotal,
            QuantidadeParcelas = Math.Max(1, dto.QuantidadeParcelas),
            MesInicial = dto.MesInicial,
            AnoInicial = dto.AnoInicial,
            Observacao = dto.Observacao,
            CriadoEm = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CriarReceitaAsync(ReceitaMensalDto dto, CancellationToken cancellationToken = default)
    {
        _context.ReceitasMensais.Add(new ReceitaMensal
        {
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            Mes = dto.Mes,
            Ano = dto.Ano
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CriarOuAtualizarFaturaAsync(FaturaCartaoCreditoDto dto, CancellationToken cancellationToken = default)
    {
        var cartao = string.IsNullOrWhiteSpace(dto.Cartao) ? "ITAU" : dto.Cartao.Trim().ToUpperInvariant();
        var fatura = await _context.FaturasCartaoCredito.FirstOrDefaultAsync(f =>
            f.Cartao == cartao && f.Ano == dto.Ano && f.Mes == dto.Mes,
            cancellationToken);

        if (fatura is null)
        {
            _context.FaturasCartaoCredito.Add(new FaturaCartaoCredito
            {
                Cartao = cartao,
                ValorTotal = dto.ValorTotal,
                Mes = dto.Mes,
                Ano = dto.Ano,
                Vencimento = dto.Vencimento,
                Observacao = dto.Observacao
            });
        }
        else
        {
            fatura.ValorTotal = dto.ValorTotal;
            fatura.Vencimento = dto.Vencimento;
            fatura.Observacao = dto.Observacao;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverLancamentoAsync(int id, CancellationToken cancellationToken = default)
    {
        var lancamento = await _context.LancamentosCartaoCredito.FindAsync([id], cancellationToken);
        if (lancamento is null)
        {
            return;
        }

        _context.LancamentosCartaoCredito.Remove(lancamento);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task GarantirReceitasPadraoAsync(int ano, CancellationToken cancellationToken)
    {
        if (await _context.ReceitasMensais.AnyAsync(r => r.Ano == ano, cancellationToken))
        {
            return;
        }

        var receitas = new List<ReceitaMensal>();
        for (var mes = 1; mes <= 12; mes++)
        {
            receitas.Add(new ReceitaMensal { Ano = ano, Mes = mes, Descricao = "SALARIO LIQUIDO TCS", Valor = 7586.34m });
            receitas.Add(new ReceitaMensal { Ano = ano, Mes = mes, Descricao = "SALARIO LIQUIDO OTICAS BRASIL", Valor = mes == 1 ? 1250m : 5125m });
        }

        receitas.Add(new ReceitaMensal { Ano = ano, Mes = 7, Descricao = "13 SALARIO LIQUIDO / FERIAS", Valor = 4500m });

        _context.ReceitasMensais.AddRange(receitas);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static decimal ValorParcelaNoMes(LancamentoCartaoCredito lancamento, int ano, int mes)
    {
        var inicio = lancamento.AnoInicial * 12 + lancamento.MesInicial;
        var atual = ano * 12 + mes;
        var parcela = atual - inicio + 1;

        if (parcela < 1 || parcela > lancamento.QuantidadeParcelas)
        {
            return 0;
        }

        return Math.Round(lancamento.ValorTotal / lancamento.QuantidadeParcelas, 2);
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

    private static LancamentoCartaoCreditoDto ToDto(LancamentoCartaoCredito lancamento) => new()
    {
        Id = lancamento.Id,
        Descricao = lancamento.Descricao,
        Cartao = lancamento.Cartao,
        ValorTotal = lancamento.ValorTotal,
        QuantidadeParcelas = lancamento.QuantidadeParcelas,
        MesInicial = lancamento.MesInicial,
        AnoInicial = lancamento.AnoInicial,
        Observacao = lancamento.Observacao
    };

    private static FaturaCartaoCreditoDto ToDto(FaturaCartaoCredito fatura) => new()
    {
        Id = fatura.Id,
        Cartao = fatura.Cartao,
        ValorTotal = fatura.ValorTotal,
        Mes = fatura.Mes,
        Ano = fatura.Ano,
        Vencimento = fatura.Vencimento,
        Observacao = fatura.Observacao
    };
}
