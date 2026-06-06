namespace MeuProjetoFinanceiro.Application.Dtos;

public class ControleMensalLinhaDto
{
    public string Grupo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public IReadOnlyList<decimal?> Valores { get; set; } = Array.Empty<decimal?>();
    public bool PodeEditar { get; set; }
    public bool PodeExcluir { get; set; }
    public bool Destaque { get; set; }
    public bool NegativoRuim { get; set; }
}
