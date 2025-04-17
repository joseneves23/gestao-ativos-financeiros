namespace AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;
using System.Linq;             

public class FiltroMontanteMaximo : IAtivoFiltro
{
    private readonly decimal? _montanteMaximo;

    public FiltroMontanteMaximo(decimal? montanteMaximo)
    {
        _montanteMaximo = montanteMaximo;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (_montanteMaximo.HasValue)
        {
            query = query.Where(a => a.ValorInicial <= _montanteMaximo);
        }
        return query;
    }
}