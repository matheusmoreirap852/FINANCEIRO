using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;

namespace MeuProjetoFinanceiro.API.Controllers;

public class ContasController : BaseController<ContaFinanceiraDto>
{
    public ContasController(ICrudService<ContaFinanceiraDto> service) : base(service)
    {
    }
}
