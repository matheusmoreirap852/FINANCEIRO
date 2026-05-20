using MeuProjetoFinanceiro.Core.Common;

namespace MeuProjetoFinanceiro.Core.Entities;

public class LancamentoCartaoCredito : IEntity
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Cartao { get; set; } = "Cartao geral";
    public decimal ValorTotal { get; set; }
    public int QuantidadeParcelas { get; set; } = 1;
    public int MesInicial { get; set; }
    public int AnoInicial { get; set; }
    public string? Observacao { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
