using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AtivosFinanceiros.Controllers;

[Authorize]
public class AtivosController : Controller
{
    private readonly MeuDbContext _context;
    private readonly ILogger<AtivosController> _logger;

    public AtivosController(MeuDbContext context, ILogger<AtivosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult EditAtivo(Guid id)
    {
        var ativo = _context.Ativos
            .Include(a => a.DepositoPrazos)
            .Include(a => a.FundoInvestimentos)
            .Include(a => a.ImovelArrendados)
            .SingleOrDefault(a => a.AtivoUuid == id);

        if (ativo == null)
        {
            return NotFound();
        }

        return View(ativo);
    }

        [HttpPost]
    public IActionResult EditAtivo(
        Ativo model,
        string? Banco,
        string? NumeroConta,
        decimal ValorInicial,
        decimal? JurosPadrao,
        decimal? JurosMensal,
        decimal? MonteInvestido,
        string? Localizacao,
        decimal? ValorImovel,
        decimal? ValorRenda,
        decimal? ValorCondominio,
        decimal? DespesasAnuais)
    {
        // Retrieve UserUuid from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            TempData["ErrorMessage"] = "User session expired or invalid.";
            _logger.LogError("Session error: User session expired or invalid or UserUuid not found.");
            return View(model);
        }

        // Set UserUuid and User
        model.UserUuid = Guid.Parse(userIdClaim.Value);
        model.User = _context.Usuarios.SingleOrDefault(u => u.UserUuid == model.UserUuid);
        if (model.User == null)
        {
            _logger.LogError("User not found for UserUuid: {0}", model.UserUuid);
            ModelState.AddModelError("User", "User not found.");
            return View(model);
        }

        // Remove the validation error for the User field
        ModelState.Remove("User");

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("ModelState Error: {0}", error.ErrorMessage);
            }
            return View(model);
        }

        var ativo = _context.Ativos.SingleOrDefault(a => a.AtivoUuid == model.AtivoUuid);
        if (ativo == null)
        {
            return NotFound();
        }

        ativo.Nome = model.Nome;
        ativo.DataInicio = model.DataInicio;
        ativo.DuracaoMeses = model.DuracaoMeses;
        ativo.ValorInicial = model.ValorInicial;
        ativo.ImpostoPerc = model.ImpostoPerc;
        ativo.LucroTotal = model.LucroTotal;
        ativo.UserUuid = model.UserUuid;

        _logger.LogInformation(model.TipoAtivo);
        // Update specific fields based on TipoAtivo
        if (model.TipoAtivo == "DepositoPrazo")
        {
            var deposito = _context.DepositoPrazos.SingleOrDefault(d => d.AtivoUuid == ativo.AtivoUuid);
            if (deposito == null)
            {
                deposito = new DepositoPrazo
                {
                    AtivoUuid = ativo.AtivoUuid,
                };
                deposito.Banco = Banco ?? deposito.Banco;
                deposito.NumeroConta = NumeroConta ?? deposito.NumeroConta;
                deposito.ValorInicial = model.ValorInicial;
                _context.DepositoPrazos.Add(deposito);
            }
            else
            {
                deposito.Banco = Banco ?? deposito.Banco;
                deposito.NumeroConta = NumeroConta ?? deposito.NumeroConta;
                deposito.ValorInicial = model.ValorInicial;
            }
        }
        else if (model.TipoAtivo == "FundoInvestimento")
        {
            var fundo = _context.FundoInvestimentos.SingleOrDefault(f => f.AtivoUuid == ativo.AtivoUuid);
            if (fundo == null)
            {
                fundo = new FundoInvestimento
                {
                    AtivoUuid = ativo.AtivoUuid,
                };
                fundo.JurosPadrao = JurosPadrao ?? fundo.JurosPadrao;
                fundo.JurosMensal = JurosMensal ?? fundo.JurosMensal;
                fundo.MonteInvestido = MonteInvestido ?? fundo.MonteInvestido;
                _context.FundoInvestimentos.Add(fundo);
            }
            else
            {
                fundo.JurosPadrao = JurosPadrao ?? fundo.JurosPadrao;
                fundo.JurosMensal = JurosMensal ?? fundo.JurosMensal;
                fundo.MonteInvestido = MonteInvestido ?? fundo.MonteInvestido;
            }
            
        }
        else if (model.TipoAtivo == "ImovelArrendado")
        {
            var imovel = _context.ImovelArrendados.SingleOrDefault(i => i.AtivoUuid == ativo.AtivoUuid);
            if (imovel == null)
            {
                imovel = new ImovelArrendado
                {
                    AtivoUuid = ativo.AtivoUuid,
                };  
                imovel.Localizacao = Localizacao ?? imovel.Localizacao;
                imovel.ValorImovel = ValorImovel ?? imovel.ValorImovel;
                imovel.ValorRenda = ValorRenda ?? imovel.ValorRenda;
                imovel.ValorCondominio = ValorCondominio ?? imovel.ValorCondominio;
                imovel.DespesasAnuais = DespesasAnuais ?? imovel.DespesasAnuais;
                _context.ImovelArrendados.Add(imovel);
            }
            else
            {
                imovel.Localizacao = Localizacao ?? imovel.Localizacao;
                imovel.ValorImovel = ValorImovel ?? imovel.ValorImovel;
                imovel.ValorRenda = ValorRenda ?? imovel.ValorRenda;
                imovel.ValorCondominio = ValorCondominio ?? imovel.ValorCondominio;
                imovel.DespesasAnuais = DespesasAnuais ?? imovel.DespesasAnuais;
            }
        }

        _context.SaveChanges();
        return RedirectToAction("MeusAtivos", "Ativos");
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
            _logger.LogInformation($"Ativo criado com sucesso: {ativo.AtivoUuid}");
            
            

            return ativo.TipoAtivo switch
            {
                "ImovelArrendado" => RedirectToAction("CreateImovelArrendado", new { ativoUuid = ativo.AtivoUuid }),
                "FundoInvestimento" => RedirectToAction("CreateFundoInvestimento", new { ativoUuid = ativo.AtivoUuid }),
                _ => RedirectToAction("MeusAtivos")
            };
            
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao criar: " + ex.Message;
            _logger.LogError(ex, "Exception while creating asset.");
            return View("CreateAtivoo", ativo);
        }
    }
    
    public IActionResult MeusAtivos(string nome, string tipo, decimal? montanteMinimo, decimal? montanteMaximo)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);

        var query = _context.Ativos.Where(a => a.UserUuid == userId);

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(a => a.Nome.Contains(nome));

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(a => a.TipoAtivo == tipo);
        
        if (montanteMinimo.HasValue || montanteMaximo.HasValue)
        {
            if (montanteMinimo.HasValue)
                query = query.Where(a => a.ValorInicial >= montanteMinimo);

            if (montanteMaximo.HasValue)
                query = query.Where(a => a.ValorInicial <= montanteMaximo);
        }
        
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroTipo = tipo;
        ViewBag.FiltroMontanteMinimo = montanteMinimo;
        ViewBag.FiltroMontanteMaximo = montanteMaximo;

        return View(query.ToList());
    }

    public IActionResult CreateFundoInvestimento(Guid ativoUuid)
    {
        ViewBag.AtivoUuid = ativoUuid;
        return View();
    }

    [HttpPost]
    public IActionResult CreateFundoInvestimento(FundoInvestimento fundoInvestimento)
    {
        try
        {
            _context.FundoInvestimentos.Add(fundoInvestimento);
            _context.SaveChanges();
            TempData["Message"] = "Fundo de investimento adicionado com sucesso!";
            return RedirectToAction("MeusAtivos");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao guardar o fundo de investimento: " + ex.Message;
            return View(fundoInvestimento);
        }
    }
    
    public IActionResult CreateImovelArrendado(Guid ativoUuid)
    {
        ViewBag.AtivoUuid = ativoUuid;
        return View();
    }
    
    [HttpPost]
    public IActionResult CreateImovelArrendado(ImovelArrendado imovel)
    {
        try
        {
            _context.ImovelArrendados.Add(imovel);
            _context.SaveChanges();
            TempData["Message"] = "Imóvel arrendado adicionado com sucesso!";
            return RedirectToAction("MeusAtivos");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Erro ao guardar o imóvel: " + ex.Message;
            return View(imovel);
        }
    }
    
    public IActionResult DetalhesAtivo(Guid id)
    {
        var ativo = _context.Ativos.FirstOrDefault(a => a.AtivoUuid == id);
        if (ativo == null)
        {
            return NotFound();
        }
        
        if (ativo.TipoAtivo == "ImovelArrendado")
        {
            ViewBag.Imovel = _context.ImovelArrendados.FirstOrDefault(i => i.AtivoUuid == id);
        }
        else if (ativo.TipoAtivo == "FundoInvestimento")
        {
            ViewBag.Fundo = _context.FundoInvestimentos.FirstOrDefault(f => f.AtivoUuid == id);
        }

        return View(ativo);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAtivo(Guid id)
    {
        var ativo = _context.Ativos
            .Include(a => a.DepositoPrazos)
            .Include(a => a.FundoInvestimentos)
            .Include(a => a.ImovelArrendados)
            .SingleOrDefault(a => a.AtivoUuid == id);

        if (ativo == null)
        {
            TempData["ErrorMessage"] = "Ativo não encontrado.";
            return RedirectToAction("MeusAtivos");
        }

        try
        {
            // Remove related entities
            if (ativo.DepositoPrazos != null)
            {
                _context.DepositoPrazos.RemoveRange(ativo.DepositoPrazos);
            }
            if (ativo.FundoInvestimentos != null)
            {
                _context.FundoInvestimentos.RemoveRange(ativo.FundoInvestimentos);
            }
            if (ativo.ImovelArrendados != null)
            {
                _context.ImovelArrendados.RemoveRange(ativo.ImovelArrendados);
            }

            // Remove the Ativo
            _context.Ativos.Remove(ativo);
            _context.SaveChanges();

            TempData["Message"] = "Ativo excluído com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir o ativo.");
            TempData["ErrorMessage"] = "Erro ao excluir o ativo.";
        }

        return RedirectToAction("MeusAtivos");
    }

}