namespace AtivosFinanceiros.Controllers;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

public class AuthController : Controller
{
    private readonly MeuDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, MeuDbContext context)
    {
        _context = context;
        _logger = logger;
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
                return RedirectToAction("Login");
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
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Usuarios.SingleOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: model.Password,
                    salt: new byte[0], // No salt
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                if (hashed == user.Senha)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.UserUuid.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    _logger.LogInformation($"User logged in: {user.Username} with UUID: {user.UserUuid}");
                    return RedirectToAction("CreateAtivoo", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Login", "Auth");
    }
}