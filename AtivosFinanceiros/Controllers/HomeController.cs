using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using Microsoft.AspNetCore.Authorization;
using AtivosFinanceiros.Filters;


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
            _logger.LogInformation($"Asset created for UserUuid: {userIdClaim.Value}");
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
        
        List<IAtivoFiltro> filtros = new List<IAtivoFiltro>
        {
            new FiltroNome(nome),
            new FiltroTipo(tipo),
            new FiltroMontanteMinimo(montanteMinimo),
            new FiltroMontanteMaximo(montanteMaximo)
        };
        
        foreach (var filtro in filtros)
        {
            query = filtro.Filtrar(query);
        }
        
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroTipo = tipo;
        ViewBag.FiltroMontanteMinimo = montanteMinimo;
        ViewBag.FiltroMontanteMaximo = montanteMaximo;

        return View(query.ToList());
    }



}