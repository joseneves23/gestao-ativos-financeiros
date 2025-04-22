using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;

namespace AtivosFinanceiros.Controllers;

public class AtivosController : Controller
{
    private readonly MeuDbContext _context;

    public AtivosController(MeuDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult EditAtivo(Guid id)
    {
        var ativo = _context.Ativos
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
        decimal? JurosPadrao, 
        decimal? JurosMensal, 
        decimal? MonteInvestido, 
        string? Localizacao, 
        decimal? ValorImovel, 
        decimal? ValorRenda, 
        decimal? ValorCondominio, 
        decimal? DespesasAnuais)
    {
        if (ModelState.IsValid)
        {
            var ativo = _context.Ativos
                .SingleOrDefault(a => a.AtivoUuid == model.AtivoUuid);

            if (ativo == null)
            {
                return NotFound();
            }

            // Update common fields
            ativo.Nome = model.Nome;
            ativo.DataInicio = model.DataInicio;
            ativo.DuracaoMeses = model.DuracaoMeses;
            ativo.ValorInicial = model.ValorInicial;
            ativo.ImpostoPerc = model.ImpostoPerc;
            ativo.LucroTotal = model.LucroTotal;

            // Update specific fields based on TipoAtivo
            if (model.TipoAtivo == "DepositoPrazo")
            {
                var deposito = _context.DepositoPrazos
                    .SingleOrDefault(d => d.AtivoUuid == ativo.AtivoUuid);

                if (deposito != null)
                {
                    deposito.Banco = Banco ?? deposito.Banco;
                    deposito.NumeroConta = NumeroConta ?? deposito.NumeroConta;
                }
            }
            else if (model.TipoAtivo == "FundoInvestimento")
            {
                var fundo = _context.FundoInvestimentos
                    .SingleOrDefault(f => f.AtivoUuid == ativo.AtivoUuid);

                if (fundo != null)
                {
                    fundo.JurosPadrao = JurosPadrao ?? fundo.JurosPadrao;
                    fundo.JurosMensal = JurosMensal ?? fundo.JurosMensal;
                    fundo.MonteInvestido = MonteInvestido ?? fundo.MonteInvestido;
                }
            }
            else if (model.TipoAtivo == "ImovelArrendado")
            {
                var imovel = _context.ImovelArrendados
                    .SingleOrDefault(i => i.AtivoUuid == ativo.AtivoUuid);

                if (imovel != null)
                {
                    imovel.Localizacao = Localizacao ?? imovel.Localizacao;
                    imovel.ValorImovel = ValorImovel ?? imovel.ValorImovel;
                    imovel.ValorRenda = ValorRenda ?? imovel.ValorRenda;
                    imovel.ValorCondominio = ValorCondominio ?? imovel.ValorCondominio;
                    imovel.DespesasAnuais = DespesasAnuais ?? imovel.DespesasAnuais;
                }
            }

            _context.SaveChanges();
            return RedirectToAction("MeusAtivos", "Home");
        }

        return View(model);
    }
}