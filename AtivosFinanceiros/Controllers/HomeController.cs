using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using Microsoft.AspNetCore.Authorization;

namespace AtivosFinanceiros.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly MeuDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, MeuDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        bool canConnect = _context.CanConnect();
        ViewBag.CanConnect = canConnect;
        return View();
    }

    public IActionResult Privacy()
    {
        bool canConnect = _context.CanConnect();
        ViewBag.CanConnect = canConnect;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public IActionResult CreateAtivoo()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult CreateAtivo(Ativo ativo)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            TempData["ErrorMessage"] = "User session expired or invalid.";
            _logger.LogError("Session error: User session expired or invalid or UserUuid not found.");
            return View("CreateAtivoo", ativo);
        }

        try
        {
            ativo.UserUuid = Guid.Parse(userIdClaim.Value);
            _context.Ativos.Add(ativo);
            _context.SaveChanges();

            TempData["Message"] = "Ativo criado com sucesso!";
            _logger.LogInformation($"Ativo criado com sucesso: {ativo.AtivoUuid}");
            
            if (ativo.TipoAtivo == "ImovelArrendado")
            {
                return RedirectToAction("CreateImovelArrendado", new { ativoUuid = ativo.AtivoUuid });
            }

            return RedirectToAction("MeusAtivos");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao criar: " + ex.Message;
            _logger.LogError(ex, "Exception while creating asset.");
            return View("CreateAtivoo", ativo);
        }
    }
    
    public IActionResult MeusAtivos(string nome, string tipo, decimal? montanteMinimo, decimal? montanteMaximo)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);

        var query = _context.Ativos.Where(a => a.UserUuid == userId);

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(a => a.Nome.Contains(nome));

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(a => a.TipoAtivo == tipo);
        
        if (montanteMinimo.HasValue || montanteMaximo.HasValue)
        {
            if (montanteMinimo.HasValue)
                query = query.Where(a => a.ValorInicial >= montanteMinimo);

            if (montanteMaximo.HasValue)
                query = query.Where(a => a.ValorInicial <= montanteMaximo);
        }
        
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroTipo = tipo;
        ViewBag.FiltroMontanteMinimo = montanteMinimo;
        ViewBag.FiltroMontanteMaximo = montanteMaximo;

        return View(query.ToList());
    }
    
    public IActionResult CreateImovelArrendado(Guid ativoUuid)
    {
        ViewBag.AtivoUuid = ativoUuid;
        return View();
    }
    
    [HttpPost]
    public IActionResult CreateImovelArrendado(ImovelArrendado imovel)
    {
        try
        {
            _context.ImovelArrendados.Add(imovel);
            _context.SaveChanges();
            TempData["Message"] = "Imóvel arrendado adicionado com sucesso!";
            return RedirectToAction("MeusAtivos");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao salvar o imóvel: " + ex.Message;
            return View(imovel);
        }
    }
    
    public IActionResult DetalhesAtivo(Guid id)
    {
        var ativo = _context.Ativos.FirstOrDefault(a => a.AtivoUuid == id);
        if (ativo == null)
        {
            return NotFound();
        }
        
        var imovel = _context.ImovelArrendados.FirstOrDefault(i => i.AtivoUuid == id);

        ViewBag.Imovel = imovel;
        return View(ativo);
    }

    
}