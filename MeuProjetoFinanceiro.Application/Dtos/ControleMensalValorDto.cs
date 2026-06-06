namespace MeuProjetoFinanceiro.Application.Dtos;

public class ControleMensalValorDto
{
    public string Grupo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int Mes { get; set; }
    public decimal Valor { get; set; }
}
