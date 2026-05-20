namespace MeuProjetoFinanceiro.Application.Dtos;

public class ReceitaMensalDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
}
