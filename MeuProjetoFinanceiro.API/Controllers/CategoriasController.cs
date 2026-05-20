using MeuProjetoFinanceiro.Application.Dtos;
using MeuProjetoFinanceiro.Application.Interfaces;

namespace MeuProjetoFinanceiro.API.Controllers;

public class CategoriasController : BaseController<CategoriaDto>
{
    public CategoriasController(ICrudService<CategoriaDto> service) : base(service)
    {
    }
}
