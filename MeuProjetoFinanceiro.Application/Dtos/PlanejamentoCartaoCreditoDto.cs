namespace MeuProjetoFinanceiro.Application.Dtos;

public class PlanejamentoCartaoCreditoDto
{
    public int Ano { get; set; }
    public decimal ReceitaMedia { get; set; }
    public decimal MaiorFaturaProjetada { get; set; }
    public decimal MesMaisApertado { get; set; }
    public IReadOnlyList<ResumoCartaoMesDto> Meses { get; set; } = Array.Empty<ResumoCartaoMesDto>();
    public IReadOnlyList<LancamentoCartaoCreditoDto> Lancamentos { get; set; } = Array.Empty<LancamentoCartaoCreditoDto>();
    public IReadOnlyList<FaturaCartaoCreditoDto> Faturas { get; set; } = Array.Empty<FaturaCartaoCreditoDto>();
}
