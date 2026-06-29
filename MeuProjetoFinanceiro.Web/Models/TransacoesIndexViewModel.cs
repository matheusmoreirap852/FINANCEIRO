using MeuProjetoFinanceiro.Application.Dtos;

namespace MeuProjetoFinanceiro.Web.Models;

public class TransacoesIndexViewModel
{
    public IReadOnlyList<TransacaoDto> Transacoes { get; set; } = [];
    public IReadOnlyList<ContaFinanceiraDto> Contas { get; set; } = [];
    public IReadOnlyList<CategoriaDto> CategoriasDespesa { get; set; } = [];
    public LancamentoLocacaoViewModel Locacao { get; set; } = new();
}

public class LancamentoLocacaoViewModel
{
    public string Descricao { get; set; } = "Locacao";
    public decimal Valor { get; set; }
    public int Dia { get; set; } = DateTime.Today.Day;
    public int Mes { get; set; } = DateTime.Today.Month;
    public int Ano { get; set; } = DateTime.Today.Year;
    public int ContaFinanceiraId { get; set; }
    public int CategoriaId { get; set; }
}
