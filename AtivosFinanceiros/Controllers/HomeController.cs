using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using Microsoft.AspNetCore.Authorization;
using AtivosFinanceiros.Facades;

namespace AtivosFinanceiros.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly MeuDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly AtivoFacade _ativoFacade;

    public HomeController(ILogger<HomeController> logger, MeuDbContext context, AtivoFacade ativoFacade)
    {
        _context = context;
        _logger = logger;
        _ativoFacade = ativoFacade;
    }

    public IActionResult Index()
    {
        ViewBag.CanConnect = _ativoFacade.PodeConectar();
        return View();
    }

    public IActionResult Privacy()
    {
        ViewBag.CanConnect = _ativoFacade.PodeConectar();
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
        if (_ativoFacade.CriarAtivo(ativo, User, out var errorMessage))
        {
            TempData["Message"] = "Ativo criado com sucesso!";
            return RedirectToAction("MeusAtivos");
        }

        TempData["ErrorMessage"] = errorMessage;
        return View("CreateAtivoo", ativo);
    }

    public IActionResult MeusAtivos(string nome, string tipo, decimal? montanteMinimo, decimal? montanteMaximo)
    {
        var ativos = _ativoFacade.ObterAtivosFiltrados(User, nome, tipo, montanteMinimo, montanteMaximo);

        ViewBag.FiltroNome = nome;
        ViewBag.FiltroTipo = tipo;
        ViewBag.FiltroMontanteMinimo = montanteMinimo;
        ViewBag.FiltroMontanteMaximo = montanteMaximo;

        return View(ativos);
    }
}
