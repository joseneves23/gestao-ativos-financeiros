namespace AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;
using System.Linq;             

public class FiltroMontanteMinimo : IAtivoFiltro
{
    private readonly decimal? _montanteMinimo;

    public FiltroMontanteMinimo(decimal? montanteMinimo)
    {
        _montanteMinimo = montanteMinimo;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (_montanteMinimo.HasValue)
        {
            query = query.Where(a => a.ValorInicial >= _montanteMinimo);
        }
        return query;
    }
}