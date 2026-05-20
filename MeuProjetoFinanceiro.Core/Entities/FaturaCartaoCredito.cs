using MeuProjetoFinanceiro.Core.Common;

namespace MeuProjetoFinanceiro.Core.Entities;

public class FaturaCartaoCredito : IEntity
{
    public int Id { get; set; }
    public string Cartao { get; set; } = "ITAU";
    public decimal ValorTotal { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
    public DateTime? Vencimento { get; set; }
    public string? Observacao { get; set; }
}
