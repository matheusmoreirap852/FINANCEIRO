using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;

namespace MeuProjetoFinanceiro.API.Controllers;

public class TransacoesController : BaseController<TransacaoDto>
{
    public TransacoesController(ICrudService<TransacaoDto> service) : base(service)
    {
    }
}
