namespace AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;
using System.Linq;             

public class FiltroTipo : IAtivoFiltro
{
    private readonly string _tipo;

    public FiltroTipo(string tipo)
    {
        _tipo = tipo;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (!string.IsNullOrEmpty(_tipo))
        {
            query = query.Where(a => a.TipoAtivo == _tipo);
        }
        return query;
    }
}