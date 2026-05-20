using MeuProjetoFinanceiro.Core.Common;
using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Core.Entities;

public class Categoria : IEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoTransacao Tipo { get; set; }
    public string Cor { get; set; } = "#2563eb";
    public bool Ativa { get; set; } = true;

    public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    public ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
}
