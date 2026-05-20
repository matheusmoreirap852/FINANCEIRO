using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<TDto> : ControllerBase
{
    private readonly ICrudService<TDto> _service;

    protected BaseController(ICrudService<TDto> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TDto dto) => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TDto dto) => Ok(await _service.UpdateAsync(dto));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => Ok(await _service.DeleteAsync(id));
}
