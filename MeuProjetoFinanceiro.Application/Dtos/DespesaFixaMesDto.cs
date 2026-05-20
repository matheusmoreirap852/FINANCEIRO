namespace MeuProjetoFinanceiro.Application.Dtos;

public class DespesaFixaMesDto
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
