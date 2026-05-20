namespace MeuProjetoFinanceiro.Application.Dtos;

public class DespesaFixaLinhaDto
{
    public string Nome { get; set; } = string.Empty;
    public IReadOnlyList<decimal> Valores { get; set; } = Array.Empty<decimal>();
    public decimal TotalAno { get; set; }
    public decimal MediaMensal { get; set; }
}
