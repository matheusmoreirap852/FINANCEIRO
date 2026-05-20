using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class DespesasFixasController : Controller
{
    private readonly IPlanejamentoDespesasFixasService _service;

    public DespesasFixasController(IPlanejamentoDespesasFixasService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int? ano, CancellationToken cancellationToken)
        => View(await _service.ObterAsync(ano ?? DateTime.Today.Year, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LancarRecorrente(LancamentoDespesaFixaDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao) || dto.Valor <= 0)
        {
            TempData["Erro"] = "Informe a descricao e o valor da despesa fixa.";
            return RedirectToAction(nameof(Index), new { ano = dto.AnoInicial });
        }

        var criadas = await _service.CriarRecorrenteAsync(dto, cancellationToken);
        TempData["Sucesso"] = $"{criadas} lancamentos de despesa fixa foram criados.";
        return RedirectToAction(nameof(Index), new { ano = dto.AnoInicial });
    }
}
