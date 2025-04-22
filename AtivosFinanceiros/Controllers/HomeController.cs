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
    
    
}