using MeuProjetoFinanceiro.Core.Enums;

namespace MeuProjetoFinanceiro.Application.Dtos;

public class ContaFinanceiraDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Instituicao { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Ativa { get; set; } = true;
}
