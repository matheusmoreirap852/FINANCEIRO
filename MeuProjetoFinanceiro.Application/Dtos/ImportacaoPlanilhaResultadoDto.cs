namespace MeuProjetoFinanceiro.Application.Dtos;

public class ImportacaoPlanilhaResultadoDto
{
    public int ReceitasImportadas { get; set; }
    public int DespesasImportadas { get; set; }
    public int TransacoesIgnoradas { get; set; }
    public string Mensagem { get; set; } = string.Empty;

    public int TotalImportado => ReceitasImportadas + DespesasImportadas;
}
