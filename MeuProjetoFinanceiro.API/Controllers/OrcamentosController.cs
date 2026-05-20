using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;

namespace MeuProjetoFinanceiro.API.Controllers;

public class OrcamentosController : BaseController<OrcamentoDto>
{
    public OrcamentosController(ICrudService<OrcamentoDto> service) : base(service)
    {
    }
}
