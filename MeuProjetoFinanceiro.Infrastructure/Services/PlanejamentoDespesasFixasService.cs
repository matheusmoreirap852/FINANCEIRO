using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Services;

public class PlanejamentoDespesasFixasService : IPlanejamentoDespesasFixasService
{
    private static readonly string[] NomesFixos =
    [
        "Prestação Casa",
        "Emprestimo",
        "Divida ativa dezembro",
        "Total pass",
        "BALAO CASA",
        "Parcela da moto",
        "Uber",
        "Casa",
        "Condominio",
        "Jiu Jitsu"
    ];

    private readonly AppDbContext _context;

    public PlanejamentoDespesasFixasService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlanejamentoDespesasFixasDto> ObterAsync(int ano, CancellationToken cancellationToken = default)
    {
        var transacoes = await _context.Transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa && t.Data.Year == ano)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var despesasFixas = transacoes
            .Where(t => EhDespesaFixa(t.Descricao))
            .ToList();

        var meses = Enumerable.Range(1, 12)
            .Select(mes => new DespesaFixaMesDto
            {
                Ano = ano,
                Mes = mes,
                Periodo = new DateTime(ano, mes, 1).ToString("MMM/yyyy"),
                Total = despesasFixas.Where(t => t.Data.Month == mes).Sum(t => t.Valor)
            })
            .ToList();

        var linhas = despesasFixas
            .GroupBy(t => NormalizarDescricao(t.Descricao))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var valores = Enumerable.Range(1, 12)
                    .Select(mes => g.Where(t => t.Data.Month == mes).Sum(t => t.Valor))
                    .ToList();

                return new DespesaFixaLinhaDto
                {
                    Nome = g.Key,
                    Valores = valores,
                    TotalAno = valores.Sum(),
                    MediaMensal = valores.Average()
                };
            })
            .ToList();

        return new PlanejamentoDespesasFixasDto
        {
            Ano = ano,
            TotalAno = meses.Sum(m => m.Total),
            MediaMensal = meses.Average(m => m.Total),
            MaiorMes = meses.Max(m => m.Total),
            Meses = meses,
            Linhas = linhas
        };
    }

    public async Task<int> CriarRecorrenteAsync(LancamentoDespesaFixaDto dto, CancellationToken cancellationToken = default)
    {
        var conta = await ObterOuCriarContaAsync(cancellationToken);
        var categoria = await ObterOuCriarCategoriaAsync(cancellationToken);
        var inicio = dto.AnoInicial * 12 + dto.MesInicial;
        var fim = dto.AnoFinal * 12 + dto.MesFinal;
        var criadas = 0;

        if (fim < inicio)
        {
            return 0;
        }

        for (var cursor = inicio; cursor <= fim; cursor++)
        {
            var ano = (cursor - 1) / 12;
            var mes = ((cursor - 1) % 12) + 1;
            var data = new DateTime(ano, mes, 1);
            var descricao = $"{dto.Descricao} - {data:MM/yyyy}";

            var existe = await _context.Transacoes.AnyAsync(t =>
                t.Descricao == descricao
                && t.Tipo == TipoTransacao.Despesa
                && t.Data == data
                && t.Valor == dto.Valor,
                cancellationToken);

            if (existe)
            {
                continue;
            }

            _context.Transacoes.Add(new Transacao
            {
                Descricao = descricao,
                Valor = dto.Valor,
                Data = data,
                Tipo = TipoTransacao.Despesa,
                Efetivada = true,
                Observacao = "Despesa fixa recorrente",
                ContaFinanceiraId = conta.Id,
                CategoriaId = categoria.Id
            });
            criadas++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return criadas;
    }

    public static bool EhDespesaFixa(string descricao)
    {
        var texto = NormalizarDescricao(descricao);
        return NomesFixos.Any(nome => texto.Equals(nome, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizarDescricao(string descricao)
    {
        var texto = descricao.Trim();
        var separador = texto.LastIndexOf(" - ", StringComparison.Ordinal);
        var nome = separador > 0 ? texto[..separador].Trim() : texto;
        return nome.Equals("Uber", StringComparison.OrdinalIgnoreCase) ? "Parcela da moto" : nome;
    }

    private async Task<ContaFinanceira> ObterOuCriarContaAsync(CancellationToken cancellationToken)
    {
        var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Nome == "Conta Principal", cancellationToken);
        if (conta is not null)
        {
            return conta;
        }

        conta = new ContaFinanceira
        {
            Nome = "Conta Principal",
            Instituicao = "Controle Financeiro",
            Tipo = TipoConta.Corrente,
            SaldoInicial = 0,
            Ativa = true
        };

        _context.ContasFinanceiras.Add(conta);
        await _context.SaveChangesAsync(cancellationToken);
        return conta;
    }

    private async Task<Categoria> ObterOuCriarCategoriaAsync(CancellationToken cancellationToken)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == "Despesas Fixas", cancellationToken);
        if (categoria is not null)
        {
            return categoria;
        }

        categoria = new Categoria
        {
            Nome = "Despesas Fixas",
            Tipo = TipoTransacao.Despesa,
            Cor = "#0f766e",
            Ativa = true
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync(cancellationToken);
        return categoria;
    }
}
