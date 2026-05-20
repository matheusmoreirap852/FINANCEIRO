using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardFinanceiroService _dashboard;

    public DashboardController(IDashboardFinanceiroService dashboard)
    {
        _dashboard = dashboard;
    }

    public async Task<IActionResult> Index(DateTime? inicio, DateTime? fim)
    {
        ViewBag.Inicio = inicio?.ToString("yyyy-MM-dd");
        ViewBag.Fim = fim?.ToString("yyyy-MM-dd");
        return View(await _dashboard.GetResumoAsync(inicio, fim));
    }
}
