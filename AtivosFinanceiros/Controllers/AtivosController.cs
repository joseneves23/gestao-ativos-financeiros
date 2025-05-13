using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

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

    
    public decimal CalcularLucroDepositoPrazo(decimal valorInicial, decimal taxaAnual, int duracaoMeses, decimal imposto)
    {
        decimal lucroBruto = valorInicial * (decimal)Math.Pow((double)(1 + taxaAnual / 100), (double)duracaoMeses / 12) - valorInicial;
        return lucroBruto - (lucroBruto * imposto / 100);
    }

    public decimal CalcularLucroFundoInvestimento(decimal monteInvestido, decimal? jurosMensal, int duracaoMeses, decimal imposto)
    {
        decimal lucroBruto = monteInvestido * (jurosMensal ?? 0) / 100 * duracaoMeses;
        return lucroBruto - (lucroBruto * imposto / 100);
    }

    public decimal CalcularLucroImovelArrendado(decimal valorRenda, decimal valorCondominio, decimal despesasAnuais, int duracaoMeses, decimal imposto)
    {
        decimal lucroBruto = (valorRenda - valorCondominio - despesasAnuais / 12) * duracaoMeses;
        return lucroBruto - (lucroBruto * imposto / 100);
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
        decimal? ValorAtual,
        decimal? JurosPadrao,
        decimal? JurosMensal,
        decimal? MonteInvestido,
        string? Localizacao,
        decimal? ValorImovel,
        decimal? ValorRenda,
        decimal? ValorCondominio,
        decimal? DespesasAnuais,
        string? Titulares,
        decimal? TaxaAnual)
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
                deposito.Titulares = Titulares ?? deposito.Titulares;
                deposito.TaxaAnual = TaxaAnual ?? deposito.TaxaAnual;
                deposito.ValorInicial = ValorAtual ?? deposito.ValorInicial;
                _context.DepositoPrazos.Add(deposito);
            }
            else
            {
                deposito.Banco = Banco ?? deposito.Banco;
                deposito.NumeroConta = NumeroConta ?? deposito.NumeroConta;
                deposito.Titulares = Titulares ?? deposito.Titulares;
                deposito.TaxaAnual = TaxaAnual ?? deposito.TaxaAnual;
                deposito.ValorInicial = ValorAtual ?? deposito.ValorInicial;
            }
            ativo.LucroTotal = CalcularLucroDepositoPrazo(
                model.ValorInicial,
                deposito.TaxaAnual ?? 0,
                model.DuracaoMeses,
                model.ImpostoPerc ?? 0);
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
            ativo.LucroTotal = CalcularLucroFundoInvestimento(
                MonteInvestido ?? 0,
                fundo.JurosMensal,
                model.DuracaoMeses,
                model.ImpostoPerc ?? 0);
            
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
            ativo.LucroTotal = CalcularLucroImovelArrendado(
                ValorRenda ?? 0,
                ValorCondominio ?? 0,
                DespesasAnuais ?? 0,
                model.DuracaoMeses,
                model.ImpostoPerc ?? 0);
        }

        _context.SaveChanges();
        return RedirectToAction("MeusAtivos", "Ativos");
    }
    public IActionResult CreateAtivoo()
    {
        TempData.Keep("Ativo");
        var ativo = TempData["Ativo"] != null 
            ? JsonConvert.DeserializeObject<Ativo>(TempData["Ativo"].ToString()) 
            : new Ativo();

        return View(ativo);
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
        ModelState.Remove("User");
        
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("ModelState Error: {0}", error.ErrorMessage);
            }
            TempData["ErrorMessage"] = "Preencha todos os campos obrigatórios.";
            return View("CreateAtivoo", ativo);
        }
        // Set UserUuid and User
        ativo.UserUuid = Guid.Parse(userIdClaim.Value);
        ativo.User = _context.Usuarios.SingleOrDefault(u => u.UserUuid == ativo.UserUuid);
        if (ativo.User == null)
        {
            _logger.LogError("User not found for UserUuid: {0}", ativo.UserUuid);
            ModelState.AddModelError("User", "User not found.");
            return View(ativo);
        }
        // Verifica se o UserUuid é válido
        if (ativo.UserUuid == Guid.Empty)
        {
            _logger.LogError("UserUuid está vazio ou inválido.");
            TempData["ErrorMessage"] = "Erro ao salvar o ativo: usuário inválido.";
            return RedirectToAction("CreateAtivoo");
        }
        // Recupera o usuário associado
        var user = _context.Usuarios.SingleOrDefault(u => u.UserUuid == ativo.UserUuid);
        if (user == null)
        {
            _logger.LogError("Usuário não encontrado para UserUuid: {0}", ativo.UserUuid);
            TempData["ErrorMessage"] = "Erro ao salvar o ativo: usuário não encontrado.";
            return RedirectToAction("CreateAtivoo");
        }
        // Associa o usuário ao ativo
        ativo.User = user;
        // Remove the validation error for the User field
        ModelState.Remove("User");

        // Armazena os dados do Ativo temporariamente
        TempData["Ativo"] = JsonConvert.SerializeObject(ativo);
        
        // Redireciona para a criação do tipo específico
        return ativo.TipoAtivo switch
        {
            "ImovelArrendado" => RedirectToAction("CreateImovelArrendado"),
            "FundoInvestimento" => RedirectToAction("CreateFundoInvestimento"),
            "DepositoPrazo" => RedirectToAction("CreateDepositoPrazo"),
            _ => RedirectToAction("CreateAtivoo", ativo)
        };
    }
    
    public IActionResult ConfirmarAtivo()
    {
        if (TempData["Ativo"] == null)
        {
            _logger.LogError("TempData[\"Ativo\"] está vazio ou nulo.");
            TempData["ErrorMessage"] = "Erro ao salvar os dados. Nenhum ativo encontrado.";
            return RedirectToAction("CreateAtivoo");
        }

        TempData.Keep("Ativo");
        var ativo = JsonConvert.DeserializeObject<Ativo>(TempData["Ativo"].ToString());

        object tipoEspecifico = null;

        switch (ativo.TipoAtivo)
        {
            case "ImovelArrendado":
                TempData.Keep("Imovel");
                var imovel = JsonConvert.DeserializeObject<ImovelArrendado>(TempData["Imovel"]?.ToString() ?? string.Empty);
                ViewBag.Imovel = imovel; // Adiciona ao ViewBag
                tipoEspecifico = imovel;
                break;
            case "FundoInvestimento":
                TempData.Keep("Fundo");
                var fundo = JsonConvert.DeserializeObject<FundoInvestimento>(TempData["Fundo"]?.ToString() ?? string.Empty);
                ViewBag.Fundo = fundo; // Adiciona ao ViewBag
                tipoEspecifico = fundo;
                break;
            case "DepositoPrazo":
                TempData.Keep("Deposito");
                var deposito = JsonConvert.DeserializeObject<DepositoPrazo>(TempData["Deposito"]?.ToString() ?? string.Empty);
                ViewBag.Deposito = deposito; // Adiciona ao ViewBag
                tipoEspecifico = deposito;
                break;
            default:
                TempData["ErrorMessage"] = "Tipo de ativo inválido.";
                return RedirectToAction("CreateAtivoo");
        }

        if (tipoEspecifico == null)
        {
            TempData["ErrorMessage"] = "Erro ao recuperar os dados do tipo específico. Tente novamente.";
            return RedirectToAction("CreateAtivoo");
        }

        ViewBag.Ativo = ativo;
        ViewBag.TipoEspecifico = tipoEspecifico;

        return View();
    }
    
    
    [HttpPost]
    
    [HttpPost]
    public IActionResult ConfirmarAtivoSalvar()
    {
        try
        {
            if (TempData["Ativo"] == null)
            {
                _logger.LogError("TempData[\"Ativo\"] está vazio ou nulo.");
                TempData["ErrorMessage"] = "Erro ao salvar os dados. Nenhum ativo encontrado.";
                return RedirectToAction("CreateAtivoo");
            }

            var ativo = JsonConvert.DeserializeObject<Ativo>(TempData["Ativo"].ToString());

            if (ativo == null)
            {
                _logger.LogError("Falha ao desserializar TempData[\"Ativo\"].");
                TempData["ErrorMessage"] = "Erro ao salvar os dados. Tente novamente.";
                return RedirectToAction("CreateAtivoo");
            }

            // Verifica se o UserUuid é válido
            if (ativo.UserUuid == Guid.Empty)
            {
                _logger.LogError("UserUuid está vazio ou inválido.");
                TempData["ErrorMessage"] = "Erro ao salvar o ativo: usuário inválido.";
                return RedirectToAction("CreateAtivoo");
            }

            // Recupera o usuário associado
            var user = _context.Usuarios.SingleOrDefault(u => u.UserUuid == ativo.UserUuid);
            if (user == null)
            {
                _logger.LogError("Usuário não encontrado para UserUuid: {0}", ativo.UserUuid);
                TempData["ErrorMessage"] = "Erro ao salvar o ativo: usuário não encontrado.";
                return RedirectToAction("CreateAtivoo");
            }

            // Associa o usuário ao ativo
            ativo.User = user;

            // Salva o ativo no banco de dados
            _context.Ativos.Add(ativo);
            _context.SaveChanges();
            _logger.LogInformation("Ativo salvo com sucesso: {0}", ativo.Nome);

            // Salva os dados específicos do tipo de ativo
            switch (ativo.TipoAtivo)
            {
                case "ImovelArrendado":
                    var imovel = JsonConvert.DeserializeObject<ImovelArrendado>(TempData["Imovel"]?.ToString() ?? string.Empty);
                    if (imovel != null)
                    {
                        imovel.AtivoUuid = ativo.AtivoUuid;
                        _context.ImovelArrendados.Add(imovel);
                    }

                    if (imovel != null)
                        ativo.LucroTotal = CalcularLucroImovelArrendado(
                            imovel.ValorRenda,
                            imovel.ValorCondominio,
                            imovel.DespesasAnuais,
                            ativo.DuracaoMeses,
                            ativo.ImpostoPerc ?? 0);
                    
                    break;

                case "FundoInvestimento":
                    var fundo = JsonConvert.DeserializeObject<FundoInvestimento>(TempData["Fundo"]?.ToString() ?? string.Empty);
                    if (fundo != null)
                    {
                        fundo.AtivoUuid = ativo.AtivoUuid;
                        _context.FundoInvestimentos.Add(fundo);
                    }
                    if (fundo != null)
                        ativo.LucroTotal = CalcularLucroFundoInvestimento(
                            fundo.MonteInvestido,
                            fundo.JurosMensal,
                            ativo.DuracaoMeses,
                            ativo.ImpostoPerc ?? 0);
                    break;

                case "DepositoPrazo":
                    var deposito = JsonConvert.DeserializeObject<DepositoPrazo>(TempData["Deposito"]?.ToString() ?? string.Empty);
                    if (deposito != null)
                    {
                        deposito.AtivoUuid = ativo.AtivoUuid;
                        _context.DepositoPrazos.Add(deposito);
                    }
                    if (deposito != null)
                        ativo.LucroTotal = CalcularLucroDepositoPrazo(
                            ativo.ValorInicial,
                            deposito.TaxaAnual ?? 0,
                            ativo.DuracaoMeses,
                            ativo.ImpostoPerc ?? 0);
                    break;

                default:
                    _logger.LogError("Tipo de ativo inválido: {0}", ativo.TipoAtivo);
                    TempData["ErrorMessage"] = "Tipo de ativo inválido.";
                    return RedirectToAction("CreateAtivoo");
            }

            _context.SaveChanges();
            TempData.Remove("Ativo");
            TempData.Remove("Imovel");
            TempData.Remove("Fundo");
            TempData.Remove("Deposito");
            TempData["Message"] = "Ativo criado com sucesso!";
            return RedirectToAction("MeusAtivos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar o ativo.");
            TempData["ErrorMessage"] = "Erro ao salvar o ativo: " + ex.Message;
            return RedirectToAction("CreateAtivoo");
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
        TempData.Keep("Fundo"); // Preserva os dados
        var fundo = TempData["Fundo"] != null
            ? JsonConvert.DeserializeObject<FundoInvestimento>(TempData["Fundo"].ToString())
            : new FundoInvestimento { AtivoUuid = ativoUuid };

        ViewBag.AtivoUuid = ativoUuid;
        return View(fundo);
    }

    [HttpPost]
    public IActionResult CreateFundoInvestimento(FundoInvestimento fundoInvestimento)
    {
        ModelState.Remove("AtivoUu");

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("ModelState Error: {0}", error.ErrorMessage);
            }
            TempData["ErrorMessage"] = "Preencha todos os campos obrigatórios.";
            return View(fundoInvestimento);
        }

        TempData["Fundo"] = JsonConvert.SerializeObject(fundoInvestimento);
        return RedirectToAction("ConfirmarAtivo");
    }
    
    public IActionResult CreateImovelArrendado(Guid ativoUuid)
    {
        TempData.Keep("Imovel");
        var imovel = TempData["Imovel"] != null
            ? JsonConvert.DeserializeObject<ImovelArrendado>(TempData["Imovel"].ToString())
            : new ImovelArrendado { AtivoUuid = ativoUuid };

        ViewBag.AtivoUuid = ativoUuid;
        return View(imovel);
    }
    
    [HttpPost]
    public IActionResult CreateImovelArrendado(ImovelArrendado imovel)
    {
        ModelState.Remove("AtivoUu");

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("ModelState Error: {0}", error.ErrorMessage);
            }
            TempData["ErrorMessage"] = "Preencha todos os campos obrigatórios.";
            return View(imovel);
        }

        TempData["Imovel"] = JsonConvert.SerializeObject(imovel);
        return RedirectToAction("ConfirmarAtivo");
    }
    
    public IActionResult CreateDepositoPrazo(Guid ativoUuid)
    {
        TempData.Keep("Deposito");
        var deposito = TempData["Deposito"] != null
            ? JsonConvert.DeserializeObject<DepositoPrazo>(TempData["Deposito"].ToString())
            : new DepositoPrazo { AtivoUuid = ativoUuid };

        _logger.LogInformation("Deposito prazo: {0}", deposito?.Banco);
        ViewBag.AtivoUuid = ativoUuid;
        return View(deposito);
    }
    [HttpPost]
    public IActionResult CreateDepositoPrazo(DepositoPrazo depositoPrazo)
    {
        ModelState.Remove("AtivoUu");

        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError("ModelState Error: {0}", error.ErrorMessage);
            }
            TempData["ErrorMessage"] = "Preencha todos os campos obrigatórios.";
            return View(depositoPrazo);
        }

        TempData["Deposito"] = JsonConvert.SerializeObject(depositoPrazo);
        return RedirectToAction("ConfirmarAtivo");
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
        else if (ativo.TipoAtivo == "DepositoPrazo")
        {
            ViewBag.Deposito = _context.DepositoPrazos.FirstOrDefault(f => f.AtivoUuid == id);
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