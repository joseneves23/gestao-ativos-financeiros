using AtivosFinanceiros.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AtivosFinanceiros.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly MeuDbContext _context;

        public AdminController(MeuDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "AdministradorPolicy")]
        public IActionResult GerirUtilizadores()
        {
            var utilizadores = _context.Usuarios.ToList();
            return View(utilizadores);
        }
        
        [Authorize(Policy = "AdministradorPolicy")]
        public IActionResult CriarUtilizador()
        {
            return View(); 
        }

        [HttpGet]
        [Authorize(Policy = "AdministradorPolicy")]
        public IActionResult EditarUtilizador(Guid id)
        {
            var utilizador = _context.Usuarios.Find(id);
            if (utilizador == null)
            {
                return NotFound();
            }

            return View(utilizador);
        }


    }
}