namespace AtivosFinanceiros.Filters;
using AtivosFinanceiros.Models;
using System.Linq;             

public class FiltroNome : IAtivoFiltro
{
    private readonly string _nome;

    public FiltroNome(string nome)
    {
        _nome = nome;
    }
    
    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (!string.IsNullOrEmpty(_nome))
        {
            query = query.Where(a => a.Nome.Contains(_nome));
        }
        return query;  
    }
}