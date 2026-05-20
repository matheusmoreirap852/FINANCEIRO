using MeuProjetoFinanceiro.Core.Common;
using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Core.Entities;

public class ContaFinanceira : IEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public decimal SaldoInicial { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
}
