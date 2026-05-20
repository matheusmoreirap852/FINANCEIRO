namespace MeuProjetoFinanceiro.Application.Dtos;

public class PlanejamentoDespesasFixasDto
{
    public int Ano { get; set; }
    public decimal TotalAno { get; set; }
    public decimal MediaMensal { get; set; }
    public decimal MaiorMes { get; set; }
    public IReadOnlyList<DespesaFixaMesDto> Meses { get; set; } = Array.Empty<DespesaFixaMesDto>();
    public IReadOnlyList<DespesaFixaLinhaDto> Linhas { get; set; } = Array.Empty<DespesaFixaLinhaDto>();
}
