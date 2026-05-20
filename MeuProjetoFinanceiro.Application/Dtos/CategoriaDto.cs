using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Application.Dtos;

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoTransacao Tipo { get; set; }
    public string Cor { get; set; } = "#2563eb";
    public bool Ativa { get; set; } = true;
}
