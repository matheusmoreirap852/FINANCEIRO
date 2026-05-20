using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.Web.Controllers;

public class CategoriasController : Controller
{
    private readonly ICrudService<CategoriaDto> _service;

    public CategoriasController(ICrudService<CategoriaDto> service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());
}
