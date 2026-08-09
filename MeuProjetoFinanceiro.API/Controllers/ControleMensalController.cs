using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MeuProjetoFinanceiro.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ControleMensalController : ControllerBase
{
    private readonly IControleMensalService _service;

    public ControleMensalController(IControleMensalService service)
    {
        _service = service;
    }

    [HttpGet("{ano:int}")]
    public async Task<IActionResult> Get(int ano, CancellationToken cancellationToken)
        => Ok(await _service.ObterAsync(ano, cancellationToken));

    [HttpPut("{ano:int}")]
    public async Task<IActionResult> Atualizar(
        int ano,
        IEnumerable<ControleMensalValorDto> valores,
        CancellationToken cancellationToken)
    {
        await _service.AtualizarValoresAsync(ano, valores, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{ano:int}")]
    public async Task<IActionResult> Remover(
        int ano,
        [FromQuery] string grupo,
        [FromQuery] string nome,
        CancellationToken cancellationToken)
    {
        await _service.RemoverLinhaAsync(ano, grupo, nome, cancellationToken);
        return NoContent();
    }
}
