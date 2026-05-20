using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class OrcamentosController : Controller
{
    private readonly ICrudService<OrcamentoDto> _service;

    public OrcamentosController(ICrudService<OrcamentoDto> service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());
}
