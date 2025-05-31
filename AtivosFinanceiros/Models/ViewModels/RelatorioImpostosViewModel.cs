namespace AtivosFinanceiros.Models.ViewModels;

public class RelatorioImpostosViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string TipoAtivo { get; set; } = string.Empty;
    public DateTime DataReferencia { get; set; }
    public string MesAno => DataReferencia.ToString("MM/yyyy");
    public double ImpostoMensal { get; set; }
}