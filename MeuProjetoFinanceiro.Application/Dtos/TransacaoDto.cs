using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Application.Dtos;

public class TransacaoDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; } = DateTime.Today;
    public TipoTransacao Tipo { get; set; }
    public bool Efetivada { get; set; } = true;
    public string? Observacao { get; set; }
    public int ContaFinanceiraId { get; set; }
    public string ContaNome { get; set; } = string.Empty;
    public int CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
}
