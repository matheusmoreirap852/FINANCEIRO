using MeuProjetoFinanceiro.Core.Common;
using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Core.Entities;

public class Transacao : IEntity
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime Data { get; set; } = DateTime.Today;
    public TipoTransacao Tipo { get; set; }
    public bool Efetivada { get; set; } = true;
    public string? Observacao { get; set; }

    public int ContaFinanceiraId { get; set; }
    public ContaFinanceira? ContaFinanceira { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
}
