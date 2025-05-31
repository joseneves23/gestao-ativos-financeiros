using Microsoft.AspNetCore.Mvc;

namespace AtivosFinanceiros.Controllers;

public class RelatoriosController : Controller
{
    public IActionResult RelatorioLucros()
    {
        return View(); 
    }

    public IActionResult RelatorioImpostos()
    {
        return View(); 
    }

    public IActionResult RelatorioAdmin()
    {
        return View(); 
    }
}