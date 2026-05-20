namespace MeuProjetoFinanceiro.Application.Dtos;

public class LancamentoCartaoCreditoDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Cartao { get; set; } = "Cartao geral";
    public decimal ValorTotal { get; set; }
    public int QuantidadeParcelas { get; set; } = 1;
    public int MesInicial { get; set; } = DateTime.Today.Month;
    public int AnoInicial { get; set; } = DateTime.Today.Year;
    public string? Observacao { get; set; }
}
