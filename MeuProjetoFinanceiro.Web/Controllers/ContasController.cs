using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class ContasController : Controller
{
    private readonly ICrudService<ContaFinanceiraDto> _service;

    public ContasController(ICrudService<ContaFinanceiraDto> service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());
}
