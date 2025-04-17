using AtivosFinanceiros.Models;

namespace AtivosFinanceiros.Filters;

public class FiltroNome : IAtivoFiltro
{
    private readonly string _nome;

    public FiltroNome(string nome)
    {
        _nome = nome;
    }

    public IQueryable<Ativo> Filtrar(IQueryable<Ativo> query)
    {
        if (string.IsNullOrEmpty(_nome)) return query;
        return query.Where(a => a.Nome.Contains(_nome));
    }
}