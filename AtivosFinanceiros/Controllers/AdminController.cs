using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using AtivosFinanceiros.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;

namespace AtivosFinanceiros.Controllers
{
    [Authorize(Policy = "AdministradorPolicy")]  // conforme suas views
    public class AdminController : Controller
    {
        private readonly MeuDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly AuthService _authService;

        public AdminController(ILogger<AdminController> logger, MeuDbContext context, AuthService authService)
        {
            _context = context;
            _logger = logger;
            _authService = authService;
        }

        // GET: /Admin/GerirUtilizadores
        public IActionResult GerirUtilizadores()
        {
            var utilizadores = _context.Usuarios.ToList();
            return View(utilizadores);
        }

        // GET: /Admin/CriarUtilizador
        public IActionResult CriarUtilizador()
        {
            return View();
        }

        // POST: /Admin/CriarUtilizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarUtilizador(Usuario user)
        {
            if (!ModelState.IsValid) return View(user);

            user.UserUuid = Guid.NewGuid();
            _authService.RegisterUserAdmin(user);

            return RedirectToAction(nameof(GerirUtilizadores));
        }

        // GET: /Admin/EditarUtilizador/{id}
        public IActionResult EditarUtilizador(Guid id)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.UserUuid == id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: /Admin/EditarUtilizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarUtilizador(Usuario user)
        {
            ModelState.Remove(nameof(user.Senha));
        
            if (!ModelState.IsValid) return View(user);

            var userDb = _context.Usuarios.FirstOrDefault(u => u.UserUuid == user.UserUuid);
            if (userDb == null) return NotFound();

            userDb.Email = user.Email;
            userDb.Username = user.Username;
            userDb.TipoPerfil = user.TipoPerfil;

            if (!string.IsNullOrEmpty(user.Senha))
            {
                // Hash da nova senha
                userDb.Senha = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: user.Senha,
                    salt: new byte[0],
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));
            }

            _context.SaveChanges();
            return RedirectToAction(nameof(GerirUtilizadores));
        }

        // POST: /Admin/Remover/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remover(Guid id)
        {
            var user = _context.Usuarios
                .Include(u => u.Ativos)
                .ThenInclude(a => a.DepositoPrazos)
                .Include(u => u.Ativos)
                .ThenInclude(a => a.FundoInvestimentos)
                .Include(u => u.Ativos)
                .ThenInclude(a => a.ImovelArrendados)
                .FirstOrDefault(u => u.UserUuid == id);

            if (user == null)
                return NotFound();

            // Remover ativos relacionados
            foreach (var ativo in user.Ativos)
            {
                _context.DepositoPrazos.RemoveRange(ativo.DepositoPrazos);
                _context.FundoInvestimentos.RemoveRange(ativo.FundoInvestimentos);
                _context.ImovelArrendados.RemoveRange(ativo.ImovelArrendados);
            }

            _context.Ativos.RemoveRange(user.Ativos);
            _context.Usuarios.Remove(user);

            _context.SaveChanges();

            return RedirectToAction(nameof(GerirUtilizadores));
        }
    }
}
