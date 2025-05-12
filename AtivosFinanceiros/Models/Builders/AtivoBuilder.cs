namespace AtivosFinanceiros.Models.Builders;

public class AtivoBuilder
{
    private readonly Ativo _ativo;

    public AtivoBuilder()
    {
        _ativo = new Ativo
        {
            AtivoUuid = Guid.NewGuid()
        };
    }

    public AtivoBuilder ComUserUuid(Guid userUuid)
    {
        _ativo.UserUuid = userUuid;
        return this;
    }

    public AtivoBuilder ComNome(string nome)
    {
        _ativo.Nome = nome;
        return this;
    }

    public AtivoBuilder ComDataInicio(DateOnly dataInicio)
    {
        _ativo.DataInicio = dataInicio;
        return this;
    }

    public AtivoBuilder ComDuracaoMeses(int duracaoMeses)
    {
        _ativo.DuracaoMeses = duracaoMeses;
        return this;
    }

    public AtivoBuilder ComValorInicial(decimal valorInicial)
    {
        _ativo.ValorInicial = valorInicial;
        return this;
    }

    public AtivoBuilder ComImpostoPerc(decimal? impostoPerc)
    {
        _ativo.ImpostoPerc = impostoPerc;
        return this;
    }

    public AtivoBuilder ComLucroTotal(decimal? lucroTotal)
    {
        _ativo.LucroTotal = lucroTotal;
        return this;
    }

    public AtivoBuilder ComTipoAtivo(string? tipoAtivo)
    {
        _ativo.TipoAtivo = tipoAtivo;
        return this;
    }

    public Ativo Build()
    {
        return _ativo;
    }
}
