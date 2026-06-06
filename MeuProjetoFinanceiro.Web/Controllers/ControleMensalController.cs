using System.Globalization;
using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class ControleMensalController : Controller
{
    private readonly IControleMensalService _service;
    private readonly IPlanejamentoCartaoCreditoService _planejamentoCartaoService;

    public ControleMensalController(
        IControleMensalService service,
        IPlanejamentoCartaoCreditoService planejamentoCartaoService)
    {
        _service = service;
        _planejamentoCartaoService = planejamentoCartaoService;
    }

    public async Task<IActionResult> Index(int? ano, CancellationToken cancellationToken)
        => View(await _service.ObterAsync(ano ?? DateTime.Today.Year, cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(ControleMensalEdicaoViewModel model, CancellationToken cancellationToken)
    {
        var valores = model.Valores
            .Select(v => new ControleMensalValorDto
            {
                Grupo = v.Grupo,
                Nome = v.Nome,
                Mes = v.Mes,
                Valor = ParseValor(v.Valor)
            })
            .ToList();

        await _service.AtualizarValoresAsync(model.Ano, valores, cancellationToken);
        TempData["Sucesso"] = "Valores do controle mensal atualizados.";
        return RedirectToAction(nameof(Index), new { ano = model.Ano });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LancarEntrada(ReceitaMensalDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao) || dto.Valor <= 0 || dto.Mes is < 1 or > 12)
        {
            TempData["Erro"] = "Informe a descricao, o valor e um mes valido para a entrada.";
            return RedirectToAction(nameof(Index), new { ano = dto.Ano });
        }

        await _planejamentoCartaoService.CriarReceitaAsync(dto, cancellationToken);
        TempData["Sucesso"] = "Entrada cadastrada no controle mensal.";
        return RedirectToAction(nameof(Index), new { ano = dto.Ano });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverEntrada(int ano, string nome, CancellationToken cancellationToken)
    {
        await _service.RemoverEntradaAsync(ano, nome, cancellationToken);
        TempData["Sucesso"] = "Entrada removida do controle mensal.";
        return RedirectToAction(nameof(Index), new { ano });
    }

    private static decimal ParseValor(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return 0;
        }

        var texto = valor.Trim();
        if (texto.Contains('.') && !texto.Contains(',') &&
            decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valorDecimal))
        {
            return valorDecimal;
        }

        if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out var valorAtual))
        {
            return valorAtual;
        }

        texto = texto.Replace(".", string.Empty).Replace(',', '.');
        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valorInvariante)
            ? valorInvariante
            : 0;
    }
}

public class ControleMensalEdicaoViewModel
{
    public int Ano { get; set; }
    public List<ControleMensalEdicaoItemViewModel> Valores { get; set; } = [];
}

public class ControleMensalEdicaoItemViewModel
{
    public string Grupo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int Mes { get; set; }
    public string? Valor { get; set; }
}
