namespace MeuProjetoFinanceiro.Application.Dtos;

public class FaturaCartaoCreditoDto
{
    public int Id { get; set; }
    public string Cartao { get; set; } = "ITAU";
    public decimal ValorTotal { get; set; }
    public int Mes { get; set; } = DateTime.Today.Month;
    public int Ano { get; set; } = DateTime.Today.Year;
    public DateTime? Vencimento { get; set; }
    public string? Observacao { get; set; }
}
