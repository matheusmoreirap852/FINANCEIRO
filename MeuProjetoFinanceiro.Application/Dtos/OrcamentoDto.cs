namespace MeuProjetoFinanceiro.Application.Dtos;

public class OrcamentoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal ValorPlanejado { get; set; }
    public decimal ValorRealizado { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
}
