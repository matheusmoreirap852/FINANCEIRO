using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Entities;
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
        await ConsolidarReceitasAsync(ano, cancellationToken);

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

        foreach (var grupo in receitas.GroupBy(r => NormalizarEntrada(r.Descricao)).OrderBy(g => g.Key))
        {
            linhas.Add(new ControleMensalLinhaDto
            {
                Grupo = "Entradas",
                Nome = grupo.Key,
                Valores = ValoresPorMes(meses, mes => grupo.Where(r => r.Mes == mes).Sum(r => r.Valor)),
                PodeEditar = true,
                PodeExcluir = true
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
        var nomesDespesasFixas = PlanejamentoDespesasFixasService.NomesFixosPadrao
            .Select(NormalizarDescricao)
            .Concat(despesasFixas.Select(d => NormalizarDescricao(d.Descricao)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(nome => nome)
            .ToList();

        foreach (var nomeDespesaFixa in nomesDespesasFixas)
        {
            var despesasDaLinha = despesasFixas
                .Where(d => NormalizarDescricao(d.Descricao).Equals(nomeDespesaFixa, StringComparison.OrdinalIgnoreCase))
                .ToList();
            var despesaPadrao = PlanejamentoDespesasFixasService.NomesFixosPadrao
                .Select(NormalizarDescricao)
                .Contains(nomeDespesaFixa, StringComparer.OrdinalIgnoreCase);

            linhas.Add(new ControleMensalLinhaDto
            {
                Grupo = "Despesas Fixas",
                Nome = nomeDespesaFixa,
                Valores = ValoresPorMes(meses, mes => despesasDaLinha.Where(d => d.Data.Month == mes).Sum(d => d.Valor)),
                PodeEditar = true,
                PodeExcluir = !despesaPadrao
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

        foreach (var grupoCartao in faturas.GroupBy(f => f.Cartao).OrderBy(g => g.Key))
        {
            linhas.Add(new ControleMensalLinhaDto
            {
                Grupo = "Cartao",
                Nome = grupoCartao.Key,
                Valores = ValoresPorMes(meses, mes => grupoCartao.Where(f => f.Mes == mes).Sum(f => f.ValorTotal)),
                PodeEditar = true,
                PodeExcluir = true,
                Destaque = true,
                NegativoRuim = true
            });
        }

        var totalCartao = meses.Select(mes => faturas.Where(f => f.Mes == mes).Sum(f => f.ValorTotal)).ToList();

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

    public async Task AtualizarValoresAsync(int ano, IEnumerable<ControleMensalValorDto> valores, CancellationToken cancellationToken = default)
    {
        foreach (var valor in valores)
        {
            if (valor.Mes is < 1 or > 12 || string.IsNullOrWhiteSpace(valor.Nome))
            {
                continue;
            }

            var nome = valor.Nome.Trim();
            switch (valor.Grupo)
            {
                case "Entradas":
                    await AtualizarReceitaAsync(ano, valor.Mes, nome, valor.Valor, cancellationToken);
                    break;
                case "Despesas Fixas":
                    await AtualizarDespesaFixaAsync(ano, valor.Mes, nome, valor.Valor, cancellationToken);
                    break;
                case "Cartao":
                    await AtualizarFaturaAsync(ano, valor.Mes, nome, valor.Valor, cancellationToken);
                    break;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        await ConsolidarReceitasAsync(ano, cancellationToken);
    }

    public async Task RemoverEntradaAsync(int ano, string nome, CancellationToken cancellationToken = default)
    {
        var entrada = NormalizarEntrada(nome);
        if (string.IsNullOrWhiteSpace(entrada))
        {
            return;
        }

        var receitas = await _context.ReceitasMensais
            .Where(r => r.Ano == ano)
            .ToListAsync(cancellationToken);

        var remover = receitas
            .Where(r => NormalizarEntrada(r.Descricao).Equals(entrada, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (remover.Count == 0)
        {
            return;
        }

        _context.ReceitasMensais.RemoveRange(remover);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverLinhaAsync(int ano, string grupo, string nome, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(grupo) || string.IsNullOrWhiteSpace(nome))
        {
            return;
        }

        switch (grupo.Trim())
        {
            case "Entradas":
                await RemoverEntradaAsync(ano, nome, cancellationToken);
                break;
            case "Despesas Fixas":
                await RemoverDespesaFixaAsync(ano, nome, cancellationToken);
                break;
            case "Cartao":
                await RemoverFaturaAsync(ano, nome, cancellationToken);
                break;
        }
    }

    private async Task AtualizarReceitaAsync(int ano, int mes, string descricao, decimal valor, CancellationToken cancellationToken)
    {
        var entrada = NormalizarEntrada(descricao);
        var receitas = await _context.ReceitasMensais
            .Where(r => r.Ano == ano && r.Mes == mes)
            .ToListAsync(cancellationToken);
        var receita = receitas.FirstOrDefault(r =>
            NormalizarEntrada(r.Descricao).Equals(entrada, StringComparison.OrdinalIgnoreCase));

        if (receita is null)
        {
            if (valor == 0)
            {
                return;
            }

            _context.ReceitasMensais.Add(new ReceitaMensal
            {
                Ano = ano,
                Mes = mes,
                Descricao = entrada,
                Valor = valor
            });
            return;
        }

        receita.Descricao = entrada;
        receita.Valor = valor;
    }

    private async Task RemoverFaturaAsync(int ano, string cartao, CancellationToken cancellationToken)
    {
        var nomeCartao = cartao.Trim();
        var faturas = await _context.FaturasCartaoCredito
            .Where(f => f.Ano == ano)
            .ToListAsync(cancellationToken);

        var remover = faturas
            .Where(f => f.Cartao.Equals(nomeCartao, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (remover.Count == 0)
        {
            return;
        }

        _context.FaturasCartaoCredito.RemoveRange(remover);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task AtualizarFaturaAsync(int ano, int mes, string cartao, decimal valor, CancellationToken cancellationToken)
    {
        var fatura = await _context.FaturasCartaoCredito.FirstOrDefaultAsync(f =>
            f.Ano == ano && f.Mes == mes && f.Cartao == cartao,
            cancellationToken);

        if (fatura is null)
        {
            if (valor == 0)
            {
                return;
            }

            _context.FaturasCartaoCredito.Add(new FaturaCartaoCredito
            {
                Ano = ano,
                Mes = mes,
                Cartao = cartao,
                ValorTotal = valor,
                Observacao = "Fatura editada no controle mensal"
            });
            return;
        }

        fatura.ValorTotal = valor;
    }

    private async Task AtualizarDespesaFixaAsync(int ano, int mes, string nome, decimal valor, CancellationToken cancellationToken)
    {
        var data = new DateTime(ano, mes, 1);
        var despesas = await _context.Transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa && t.Data.Year == ano && t.Data.Month == mes)
            .ToListAsync(cancellationToken);

        var despesasDaLinha = despesas
            .Where(d => PlanejamentoDespesasFixasService.EhDespesaFixa(d.Descricao)
                        && NormalizarDescricao(d.Descricao).Equals(nome, StringComparison.OrdinalIgnoreCase))
            .OrderBy(d => d.Id)
            .ToList();

        if (despesasDaLinha.Count == 0)
        {
            if (valor == 0)
            {
                return;
            }

            var conta = await ObterOuCriarContaAsync(cancellationToken);
            var categoria = await ObterOuCriarCategoriaAsync(cancellationToken);
            _context.Transacoes.Add(new Transacao
            {
                Descricao = $"{nome} - {data:MM/yyyy}",
                Valor = valor,
                Data = data,
                Tipo = TipoTransacao.Despesa,
                Efetivada = true,
                Observacao = "Despesa fixa editada no controle mensal",
                ContaFinanceiraId = conta.Id,
                CategoriaId = categoria.Id
            });
            return;
        }

        var totalAtual = despesasDaLinha.Sum(d => d.Valor);
        var diferenca = valor - totalAtual;
        despesasDaLinha[0].Valor += diferenca;
    }

    private async Task RemoverDespesaFixaAsync(int ano, string nome, CancellationToken cancellationToken)
    {
        var nomeLinha = nome.Trim();
        var despesas = await _context.Transacoes
            .Where(t => t.Tipo == TipoTransacao.Despesa && t.Data.Year == ano)
            .ToListAsync(cancellationToken);

        var remover = despesas
            .Where(d => PlanejamentoDespesasFixasService.EhDespesaFixa(d.Descricao)
                        && NormalizarDescricao(d.Descricao).Equals(nomeLinha, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (remover.Count == 0)
        {
            return;
        }

        _context.Transacoes.RemoveRange(remover);
        await _context.SaveChangesAsync(cancellationToken);
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

    private async Task ConsolidarReceitasAsync(int ano, CancellationToken cancellationToken)
    {
        var receitas = await _context.ReceitasMensais
            .Where(r => r.Ano == ano)
            .ToListAsync(cancellationToken);

        var alterou = false;
        foreach (var grupo in receitas.GroupBy(r => new { Nome = NormalizarEntrada(r.Descricao).ToUpperInvariant(), r.Ano, r.Mes }))
        {
            var registros = grupo.OrderByDescending(r => r.Valor != 0).ThenBy(r => r.Id).ToList();
            var principal = registros[0];
            var total = registros.Sum(r => r.Valor);
            var duplicados = registros.Skip(1).ToList();
            var normalizada = NormalizarEntrada(principal.Descricao);

            if (duplicados.Count == 0 && principal.Descricao != normalizada)
            {
                principal.Descricao = normalizada;
                alterou = true;
            }

            if (principal.Valor != total)
            {
                principal.Valor = total;
                alterou = true;
            }

            if (duplicados.Count > 0)
            {
                _context.ReceitasMensais.RemoveRange(duplicados);
                alterou = true;
            }
        }

        var zeradas = receitas
            .Where(r => r.Valor == 0)
            .GroupBy(r => NormalizarEntrada(r.Descricao).ToUpperInvariant())
            .Where(g => g.All(r => r.Valor == 0))
            .SelectMany(g => g)
            .ToList();

        if (zeradas.Count > 0)
        {
            _context.ReceitasMensais.RemoveRange(zeradas);
            alterou = true;
        }

        if (alterou)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
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

    private static string NormalizarEntrada(string descricao)
        => descricao.Trim();

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
