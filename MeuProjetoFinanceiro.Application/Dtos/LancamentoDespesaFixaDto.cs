namespace MeuProjetoFinanceiro.Application.Dtos;

public class LancamentoDespesaFixaDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int MesInicial { get; set; } = DateTime.Today.Month;
    public int AnoInicial { get; set; } = DateTime.Today.Year;
    public int MesFinal { get; set; } = DateTime.Today.Month;
    public int AnoFinal { get; set; } = DateTime.Today.Year;
}
