using AtivosFinanceiros.Models;

namespace AtivosFinanceiros.Filters;

public interface IAtivoFiltro
{
    IQueryable<Ativo> Filtrar(IQueryable<Ativo> query);

}