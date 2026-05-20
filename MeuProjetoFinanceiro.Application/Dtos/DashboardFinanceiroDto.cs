namespace MeuProjetoFinanceiro.Application.Dtos;

public class DashboardFinanceiroDto
{
    public decimal ReceitaTotal { get; set; }
    public decimal DespesaTotal { get; set; }
    public decimal Resultado { get; set; }
    public decimal SaldoTotal { get; set; }
    public int TransacoesPendentes { get; set; }
    public IReadOnlyList<TransacaoDto> UltimasTransacoes { get; set; } = Array.Empty<TransacaoDto>();
    public IReadOnlyList<OrcamentoDto> Orcamentos { get; set; } = Array.Empty<OrcamentoDto>();
    public IReadOnlyList<string> LabelsCategorias { get; set; } = Array.Empty<string>();
    public IReadOnlyList<decimal> ValoresCategorias { get; set; } = Array.Empty<decimal>();
}
