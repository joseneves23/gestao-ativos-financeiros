using AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;

public class FiltroMontanteMaximo : IAtivoFiltro
{
    private readonly decimal? _maximo;

    public FiltroMontanteMaximo(decimal? maximo)
    {
        _maximo = maximo;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (!_maximo.HasValue) return query;
        return query.Where(a => a.ValorInicial <= _maximo.Value);
    }
}