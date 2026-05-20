namespace MeuProjetoFinanceiro.Application.Dtos;

public class ControleMensalDto
{
    public int Ano { get; set; }
    public IReadOnlyList<string> Meses { get; set; } = Array.Empty<string>();
    public IReadOnlyList<ControleMensalLinhaDto> Linhas { get; set; } = Array.Empty<ControleMensalLinhaDto>();
}
