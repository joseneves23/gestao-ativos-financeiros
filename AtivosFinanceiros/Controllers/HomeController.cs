using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AtivosFinanceiros.Controllers;

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
    
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Register(Usuario model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Directly save the password without using a salt
                model.Senha = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: model.Senha,
                    salt: new byte[0], // No salt
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                model.TipoPerfil = "UTILIZADOR";
                // Save the user to the database
                _context.Usuarios.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error occurred while registering user.");
                ModelState.AddModelError("", "An error occurred while registering the user.");
            }
        }
        else
        {
            // Log model state errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError(error.ErrorMessage);
            }
        }
        return View(model);
    }public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Usuarios.SingleOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                // Hash the entered password
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: model.Password,
                    salt: new byte[0], // No salt
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                if (hashed == user.Senha)
                {
                    // Authentication successful
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View(model);
    }
}