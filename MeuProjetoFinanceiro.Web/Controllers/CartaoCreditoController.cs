using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class CartaoCreditoController : Controller
{
    private readonly IPlanejamentoCartaoCreditoService _service;

    public CartaoCreditoController(IPlanejamentoCartaoCreditoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int? ano, CancellationToken cancellationToken)
    {
        return View(await _service.ObterAsync(ano ?? DateTime.Today.Year, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LancarCompra(LancamentoCartaoCreditoDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao) || dto.ValorTotal <= 0)
        {
            TempData["Erro"] = "Informe a descricao e o valor total da compra.";
            return RedirectToAction(nameof(Index), new { ano = dto.AnoInicial });
        }

        await _service.CriarLancamentoAsync(dto, cancellationToken);
        TempData["Sucesso"] = "Compra lancada no planejamento do cartao.";
        return RedirectToAction(nameof(Index), new { ano = dto.AnoInicial });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LancarReceita(ReceitaMensalDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao) || dto.Valor <= 0)
        {
            TempData["Erro"] = "Informe a descricao e o valor da entrada.";
            return RedirectToAction(nameof(Index), new { ano = dto.Ano });
        }

        await _service.CriarReceitaAsync(dto, cancellationToken);
        TempData["Sucesso"] = "Entrada lancada no planejamento.";
        return RedirectToAction(nameof(Index), new { ano = dto.Ano });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverLancamento(int id, int ano, CancellationToken cancellationToken)
    {
        await _service.RemoverLancamentoAsync(id, cancellationToken);
        TempData["Sucesso"] = "Compra removida do planejamento.";
        return RedirectToAction(nameof(Index), new { ano });
    }
}
