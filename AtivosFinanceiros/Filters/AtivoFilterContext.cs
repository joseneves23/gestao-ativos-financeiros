using AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;

public class AtivoFilterContext
{
    private readonly IEnumerable<IAtivoFiltro> _filtros;

    public AtivoFilterContext(IEnumerable<IAtivoFiltro> filtros)
    {
        _filtros = filtros;
    }

    public IQueryable<Ativo> AplicarFiltros(IQueryable<Ativo> query)
    {
        foreach (var filtro in _filtros)
        {
            query = filtro.Filtrar(query);
        }

        return query;
    }
}