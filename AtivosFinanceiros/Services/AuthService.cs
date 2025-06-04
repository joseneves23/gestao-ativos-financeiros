using AtivosFinanceiros.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AtivosFinanceiros.Services;

// AuthService.cs
public class AuthService
{
    private readonly MeuDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(MeuDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Usuario ValidateUser(string email, string password)
    {
        var user = _context.Usuarios.SingleOrDefault(u => u.Email == email);
        if (user != null)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: new byte[0], // No salt
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            if (hashed == user.Senha)
            {
                return user;
            }
        }
        return null;
    }

    public void RegisterUser(Usuario user)
    {
        user.Senha = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: user.Senha,
            salt: new byte[0], // No salt
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        user.TipoPerfil = "UTILIZADOR";
        _context.Usuarios.Add(user);
        _context.SaveChanges();
    }
    
    public void RegisterUserAdmin(Usuario user)
    {
        user.Senha = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: user.Senha,
            salt: new byte[0], // No salt
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        _context.Usuarios.Add(user);
        _context.SaveChanges();
    }

}