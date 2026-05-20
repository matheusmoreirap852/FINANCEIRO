using ClosedXML.Excel;
using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;
using MeuProjetoFinanceiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Services;

public class ImportacaoPlanilhaFinanceiraService : IImportacaoPlanilhaFinanceiraService
{
    private static readonly HashSet<string> LinhasResumo = new(StringComparer.OrdinalIgnoreCase)
    {
        "Total Rendimentos Variáveis",
        "DESPESAS FIXAS",
        "CARTAO BANCO DO BRASIL",
        "ITAU",
        "Faturas Parceladas a pagar",
        "TOTAL DE DESPESAS",
        "VALOR PAGO",
        "VALOR A PAGAR",
        "SALDO FINAL",
        "Despesas",
        "Proventos Fixos",
        "Informações a cima"
    };

    private readonly AppDbContext _context;

    public ImportacaoPlanilhaFinanceiraService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ImportacaoPlanilhaResultadoDto> ImportarAsync(Stream arquivo, string nomeArquivo, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(arquivo);
        var planilha = workbook.Worksheets.FirstOrDefault(w => w.Name.Equals("MATHEUS", StringComparison.OrdinalIgnoreCase))
            ?? workbook.Worksheets.First();

        var conta = await ObterOuCriarContaAsync(cancellationToken);
        var categorias = await _context.Categorias.ToDictionaryAsync(c => c.Nome.ToUpper(), cancellationToken);
        var transacoesExistentes = await _context.Transacoes
            .Select(t => new { t.Descricao, t.Data, t.Valor, t.Tipo })
            .ToListAsync(cancellationToken);

        var resultado = new ImportacaoPlanilhaResultadoDto();
        var anoPadrao = ObterAnoPadrao(planilha);

        foreach (var linha in planilha.RowsUsed())
        {
            var descricao = linha.Cell(2).GetString().Trim();
            if (string.IsNullOrWhiteSpace(descricao) || LinhasResumo.Contains(descricao))
            {
                continue;
            }

            var numeroLinha = linha.RowNumber();
            var tipo = numeroLinha < 14 ? TipoTransacao.Receita : TipoTransacao.Despesa;

            for (var coluna = 3; coluna <= 16; coluna++)
            {
                var valorCell = linha.Cell(coluna);
                if (!valorCell.TryGetValue<decimal>(out var valor) || valor == 0)
                {
                    continue;
                }

                var data = ObterDataDaColuna(planilha.Cell(tipo == TipoTransacao.Receita ? 4 : 14, coluna), anoPadrao);
                if (data is null)
                {
                    resultado.TransacoesIgnoradas++;
                    continue;
                }

                var categoria = await ObterOuCriarCategoriaAsync(categorias, descricao, tipo, cancellationToken);
                var descricaoTransacao = $"{descricao} - {data.Value:MM/yyyy}";

                var jaExiste = transacoesExistentes.Any(t =>
                    t.Descricao.Equals(descricaoTransacao, StringComparison.OrdinalIgnoreCase)
                    && t.Data.Date == data.Value.Date
                    && t.Valor == Math.Abs(valor)
                    && t.Tipo == tipo);

                if (jaExiste)
                {
                    resultado.TransacoesIgnoradas++;
                    continue;
                }

                _context.Transacoes.Add(new Transacao
                {
                    Descricao = descricaoTransacao,
                    Valor = Math.Abs(valor),
                    Data = data.Value,
                    Tipo = tipo,
                    Efetivada = true,
                    Observacao = $"Importado de {nomeArquivo}",
                    ContaFinanceiraId = conta.Id,
                    CategoriaId = categoria.Id
                });

                transacoesExistentes.Add(new { Descricao = descricaoTransacao, Data = data.Value, Valor = Math.Abs(valor), Tipo = tipo });

                if (tipo == TipoTransacao.Receita)
                {
                    resultado.ReceitasImportadas++;
                }
                else
                {
                    resultado.DespesasImportadas++;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        resultado.Mensagem = resultado.TotalImportado == 0
            ? "Nenhuma nova transacao foi importada. Talvez a planilha ja tenha sido processada."
            : $"{resultado.TotalImportado} transacoes importadas com sucesso.";

        return resultado;
    }

    private async Task<ContaFinanceira> ObterOuCriarContaAsync(CancellationToken cancellationToken)
    {
        var conta = await _context.ContasFinanceiras.FirstOrDefaultAsync(c => c.Nome == "Planilha Financeira", cancellationToken);
        if (conta is not null)
        {
            return conta;
        }

        conta = new ContaFinanceira
        {
            Nome = "Planilha Financeira",
            Instituicao = "Importacao Excel",
            Tipo = TipoConta.Corrente,
            SaldoInicial = 0,
            Ativa = true,
            CriadaEm = DateTime.UtcNow
        };

        _context.ContasFinanceiras.Add(conta);
        await _context.SaveChangesAsync(cancellationToken);
        return conta;
    }

    private async Task<Categoria> ObterOuCriarCategoriaAsync(
        Dictionary<string, Categoria> categorias,
        string nome,
        TipoTransacao tipo,
        CancellationToken cancellationToken)
    {
        var chave = nome.ToUpper();
        if (categorias.TryGetValue(chave, out var categoria))
        {
            return categoria;
        }

        categoria = new Categoria
        {
            Nome = nome,
            Tipo = tipo,
            Cor = tipo == TipoTransacao.Receita ? "#16a34a" : "#2563eb",
            Ativa = true
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync(cancellationToken);
        categorias[chave] = categoria;
        return categoria;
    }

    private static int ObterAnoPadrao(IXLWorksheet planilha)
    {
        var atualizacao = planilha.Cell(1, 2);
        return atualizacao.TryGetValue<DateTime>(out var dataAtualizacao) ? dataAtualizacao.Year : DateTime.Today.Year;
    }

    private static DateTime? ObterDataDaColuna(IXLCell cabecalho, int anoPadrao)
    {
        if (cabecalho.TryGetValue<DateTime>(out var data))
        {
            return new DateTime(data.Year, data.Month, 1);
        }

        if (cabecalho.TryGetValue<double>(out var serial))
        {
            var serialDate = DateTime.FromOADate(serial);
            return new DateTime(serialDate.Year, serialDate.Month, 1);
        }

        var texto = cabecalho.GetString();
        if (DateTime.TryParse(texto, out var textoData))
        {
            return new DateTime(textoData.Year, textoData.Month, 1);
        }

        return null;
    }
}
