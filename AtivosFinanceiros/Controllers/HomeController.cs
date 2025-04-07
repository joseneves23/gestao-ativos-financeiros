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
            _logger.LogInformation($"Asset created for UserUuid: {userIdClaim.Value}");
            return RedirectToAction("CreateAtivoo");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao criar: " + ex.Message;
            _logger.LogError(ex, "Exception while creating asset.");
            return View("CreateAtivoo", ativo);
        }
    }






    
}