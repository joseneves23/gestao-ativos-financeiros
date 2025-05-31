namespace AtivosFinanceiros.Models.ViewModels;

public class RelatorioLucrosViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string TipoAtivo { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public int DuracaoMeses { get; set; }
    public double LucroTotalBruto { get; set; }
    public double LucroTotalLiquido { get; set; }
    public double LucroMensalBruto { get; set; }
    public double LucroMensalLiquido { get; set; }
}