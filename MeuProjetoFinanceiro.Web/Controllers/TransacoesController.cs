using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using MeuProjetoFinanceiro.Core.Enums;
using MeuProjetoFinanceiro.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class TransacoesController : Controller
{
    private readonly ICrudService<TransacaoDto> _transacoes;
    private readonly ICrudService<ContaFinanceiraDto> _contas;
    private readonly ICrudService<CategoriaDto> _categorias;

    public TransacoesController(
        ICrudService<TransacaoDto> transacoes,
        ICrudService<ContaFinanceiraDto> contas,
        ICrudService<CategoriaDto> categorias)
    {
        _transacoes = transacoes;
        _contas = contas;
        _categorias = categorias;
    }

    public async Task<IActionResult> Index()
        => View(await CriarViewModelAsync(new LancamentoLocacaoViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CadastrarLocacao(LancamentoLocacaoViewModel locacao)
    {
        DateTime data;
        try
        {
            data = new DateTime(locacao.Ano, locacao.Mes, locacao.Dia);
        }
        catch (ArgumentOutOfRangeException)
        {
            TempData["Erro"] = "Informe uma data valida para a locacao.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(locacao.Descricao) || locacao.Valor <= 0)
        {
            TempData["Erro"] = "Informe a descricao e o valor da locacao.";
            return RedirectToAction(nameof(Index));
        }

        if (locacao.ContaFinanceiraId <= 0 || locacao.CategoriaId <= 0)
        {
            TempData["Erro"] = "Selecione a conta e a categoria da locacao.";
            return RedirectToAction(nameof(Index));
        }

        await _transacoes.CreateAsync(new TransacaoDto
        {
            Descricao = locacao.Descricao.Trim(),
            Valor = locacao.Valor,
            Data = data,
            Tipo = TipoTransacao.Despesa,
            Efetivada = true,
            ContaFinanceiraId = locacao.ContaFinanceiraId,
            CategoriaId = locacao.CategoriaId
        });

        TempData["Sucesso"] = "Locacao cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<TransacoesIndexViewModel> CriarViewModelAsync(LancamentoLocacaoViewModel locacao)
    {
        var contas = (await _contas.GetAllAsync()).Where(c => c.Ativa).OrderBy(c => c.Nome).ToList();
        var categoriasDespesa = (await _categorias.GetAllAsync())
            .Where(c => c.Ativa && c.Tipo == TipoTransacao.Despesa)
            .OrderBy(c => c.Nome)
            .ToList();

        locacao.ContaFinanceiraId = locacao.ContaFinanceiraId > 0
            ? locacao.ContaFinanceiraId
            : contas.FirstOrDefault()?.Id ?? 0;
        locacao.CategoriaId = locacao.CategoriaId > 0
            ? locacao.CategoriaId
            : categoriasDespesa.FirstOrDefault(c => c.Nome.Equals("Moradia", StringComparison.OrdinalIgnoreCase))?.Id
              ?? categoriasDespesa.FirstOrDefault()?.Id
              ?? 0;

        return new TransacoesIndexViewModel
        {
            Transacoes = await _transacoes.GetAllAsync(),
            Contas = contas,
            CategoriasDespesa = categoriasDespesa,
            Locacao = locacao
        };
    }
}
