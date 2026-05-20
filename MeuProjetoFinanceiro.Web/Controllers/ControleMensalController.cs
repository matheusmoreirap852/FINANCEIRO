using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class ControleMensalController : Controller
{
    private readonly IControleMensalService _service;

    public ControleMensalController(IControleMensalService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int? ano, CancellationToken cancellationToken)
        => View(await _service.ObterAsync(ano ?? DateTime.Today.Year, cancellationToken));
}
