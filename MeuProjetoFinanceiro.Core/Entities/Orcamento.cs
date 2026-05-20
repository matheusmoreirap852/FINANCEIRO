using MeuProjetoFinanceiro.Core.Common;

namespace MeuProjetoFinanceiro.Core.Entities;

public class Orcamento : IEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal ValorPlanejado { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
}
