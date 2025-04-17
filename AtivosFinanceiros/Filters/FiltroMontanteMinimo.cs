// Filters/FiltroMontanteMinimo.cs

using AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;

public class FiltroMontanteMinimo : IAtivoFiltro
{
    private readonly decimal? _minimo;

    public FiltroMontanteMinimo(decimal? minimo)
    {
        _minimo = minimo;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (!_minimo.HasValue) return query;
        return query.Where(a => a.ValorInicial >= _minimo.Value);
    }
}