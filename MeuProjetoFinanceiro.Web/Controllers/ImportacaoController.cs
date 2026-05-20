using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class ImportacaoController : Controller
{
    private readonly IImportacaoPlanilhaFinanceiraService _importacao;

    public ImportacaoController(IImportacaoPlanilhaFinanceiraService importacao)
    {
        _importacao = importacao;
    }

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormFile? arquivo, CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Selecione uma planilha .xlsx para importar.");
            return View();
        }

        await using var stream = arquivo.OpenReadStream();
        ImportacaoPlanilhaResultadoDto resultado = await _importacao.ImportarAsync(stream, arquivo.FileName, cancellationToken);
        return View(resultado);
    }
}
