using AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;

public class FiltroTipo : IAtivoFiltro
{
    private readonly string _tipo;

    public FiltroTipo(string tipo)
    {
        _tipo = tipo;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (string.IsNullOrEmpty(_tipo)) return query;
        return query.Where(a => a.TipoAtivo == _tipo);
    }
}