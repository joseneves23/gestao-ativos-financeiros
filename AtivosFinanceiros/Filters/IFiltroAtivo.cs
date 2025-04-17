namespace AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;
using System.Linq;             

public interface IAtivoFiltro
{
    IQueryable<Ativo> Filtrar(IQueryable<Ativo> query);
}