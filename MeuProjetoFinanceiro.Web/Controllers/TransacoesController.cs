using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class TransacoesController : Controller
{
    private readonly ICrudService<TransacaoDto> _service;

    public TransacoesController(ICrudService<TransacaoDto> service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());
}
