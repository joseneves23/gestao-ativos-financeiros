﻿using AtivosFinanceiros.Services;

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
    private readonly AuthService _authService;
    private readonly MeuDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, MeuDbContext context, AuthService authService)
    {
        _authService = authService;
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
            var existingUser = _context.Usuarios.SingleOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }
            try
            {
                _authService.RegisterUser(model);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user.");
                ModelState.AddModelError("", "An error occurred while registering the user.");
            }
        }
        return View(model);
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _authService.ValidateUser(model.Email, model.Password);
            if (user != null)
            {
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserUuid.ToString()),
                    new Claim("TipoPerfil", user.TipoPerfil) // Adiciona o tipo de perfil
                };
                
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                _logger.LogInformation($"User logged in: {user.Username} with UUID: {user.UserUuid}");
                return RedirectToAction("Index", "Home");
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