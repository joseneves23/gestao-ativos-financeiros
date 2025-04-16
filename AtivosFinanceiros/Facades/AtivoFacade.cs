using AtivosFinanceiros.Models;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AtivosFinanceiros.Facades;

public class AtivoFacade
{
    private readonly MeuDbContext _context;
    private readonly ILogger<AtivoFacade> _logger;

    public AtivoFacade(MeuDbContext context, ILogger<AtivoFacade> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool PodeConectar()
    {
        return _context.CanConnect();
    }

    public bool CriarAtivo(Ativo ativo, ClaimsPrincipal user, out string? errorMessage)
    {
        errorMessage = null;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            errorMessage = "User session expired or invalid.";
            _logger.LogError("Session error: User session expired or invalid or UserUuid not found.");
            return false;
        }

        try
        {
            ativo.UserUuid = Guid.Parse(userIdClaim.Value);
            _context.Ativos.Add(ativo);
            _context.SaveChanges();
            _logger.LogInformation($"Asset created for UserUuid: {userIdClaim.Value}");
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = "Erro ao criar: " + ex.Message;
            _logger.LogError(ex, "Exception while creating asset.");
            return false;
        }
    }

    public List<Ativo> ObterAtivosFiltrados(ClaimsPrincipal user, string? nome, string? tipo, decimal? montanteMinimo, decimal? montanteMaximo)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return new List<Ativo>();

        var userId = Guid.Parse(userIdClaim.Value);
        var query = _context.Ativos.Where(a => a.UserUuid == userId);

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(a => a.Nome.Contains(nome));
        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(a => a.TipoAtivo == tipo);
        if (montanteMinimo.HasValue)
            query = query.Where(a => a.ValorInicial >= montanteMinimo);
        if (montanteMaximo.HasValue)
            query = query.Where(a => a.ValorInicial <= montanteMaximo);

        return query.ToList();
    }
}
