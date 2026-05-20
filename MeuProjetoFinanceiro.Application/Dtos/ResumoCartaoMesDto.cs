namespace MeuProjetoFinanceiro.Application.Dtos;

public class ResumoCartaoMesDto
{
    public int Mes { get; set; }
    public int Ano { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal ReceitaPrevista { get; set; }
    public decimal FaturaProjetada { get; set; }
    public decimal OutrasDespesas { get; set; }
    public decimal SaldoAposCartao => ReceitaPrevista - FaturaProjetada;
    public decimal SaldoFinalProjetado => ReceitaPrevista - FaturaProjetada - OutrasDespesas;
    public bool Estourou => SaldoFinalProjetado < 0;
}
