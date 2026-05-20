using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardFinanceiroService _dashboard;

    public DashboardController(IDashboardFinanceiroService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    public async Task<IActionResult> Get(DateTime? inicio, DateTime? fim)
        => Ok(await _dashboard.GetResumoAsync(inicio, fim));
}
