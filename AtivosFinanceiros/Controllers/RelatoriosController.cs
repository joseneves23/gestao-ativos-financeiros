using Microsoft.AspNetCore.Mvc;
using AtivosFinanceiros.Models;
using AtivosFinanceiros.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AtivosFinanceiros.Services.Reports;
using Microsoft.Extensions.Logging;

namespace AtivosFinanceiros.Controllers
{
    [Authorize]
    public class RelatoriosController : Controller
    {
        private readonly MeuDbContext _context;
        private readonly ILogger<RelatoriosController> _logger;
        private readonly PdfReportService _pdfReportService;

        public RelatoriosController(MeuDbContext context, ILogger<RelatoriosController> logger, PdfReportService pdfReportService)
        {
            _context = context;
            _logger = logger;
            _pdfReportService = pdfReportService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdClaim?.Value, out var userId) ? userId : Guid.Empty;
        }

        public IActionResult RelatorioLucros(DateTime? dataInicio, DateTime? dataFim)
        {
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

            if (!dataInicio.HasValue || !dataFim.HasValue)
            {
                return View(new List<RelatorioLucrosViewModel>());
            }

            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Tentativa de acesso sem usuário autenticado.");
                return Unauthorized();
            }

            var dataInicioOnly = DateOnly.FromDateTime(dataInicio.Value);
            var dataFimOnly = DateOnly.FromDateTime(dataFim.Value);

            var ativos = _context.Ativos
                .Where(a => a.UserUuid == userId &&
                            a.DataInicio >= dataInicioOnly &&
                            a.DataInicio <= dataFimOnly)
                .ToList();

            var relatorio = new List<RelatorioLucrosViewModel>();

            foreach (var ativo in ativos)
            {
                double lucroTotalBruto = (double)(ativo.LucroTotal ?? 0m);
                double impostoPerc = (double)(ativo.ImpostoPerc);
                double lucroTotalLiquido = lucroTotalBruto * (1 - impostoPerc / 100);

                double lucroMensalBruto = ativo.DuracaoMeses > 0 ? lucroTotalBruto / ativo.DuracaoMeses : 0;
                double lucroMensalLiquido = ativo.DuracaoMeses > 0 ? lucroTotalLiquido / ativo.DuracaoMeses : 0;

                relatorio.Add(new RelatorioLucrosViewModel
                {
                    Nome = ativo.Nome,
                    TipoAtivo = ativo.TipoAtivo ?? string.Empty,
                    DataInicio = ativo.DataInicio.ToDateTime(TimeOnly.MinValue),
                    DuracaoMeses = ativo.DuracaoMeses,
                    LucroTotalBruto = lucroTotalBruto,
                    LucroTotalLiquido = lucroTotalLiquido,
                    LucroMensalBruto = lucroMensalBruto,
                    LucroMensalLiquido = lucroMensalLiquido
                });
            }

            return View(relatorio);
        }

        public IActionResult RelatorioImpostos(DateTime? dataInicio, DateTime? dataFim)
        {
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

            if (!dataInicio.HasValue || !dataFim.HasValue)
            {
                return View(new List<RelatorioImpostosViewModel>());
            }

            var userId = GetUserId();
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Tentativa de acesso sem utilizador autenticado.");
                return Unauthorized();
            }

            var dataInicioOnly = DateOnly.FromDateTime(dataInicio.Value);
            var dataFimOnly = DateOnly.FromDateTime(dataFim.Value);

            var ativos = _context.Ativos
                .Where(a => a.UserUuid == userId && a.DataInicio <= dataFimOnly)
                .ToList();

            var relatorio = new List<RelatorioImpostosViewModel>();

            foreach (var ativo in ativos)
            {
                var dataAtivoInicio = ativo.DataInicio < dataInicioOnly ? dataInicioOnly : ativo.DataInicio;
                var meses = GetMesesEntreDatas(dataAtivoInicio, dataFimOnly);

                double impostoPerc = (double)(ativo.ImpostoPerc);
                double lucroTotalBruto = (double)(ativo.LucroTotal ?? 0m);
                double lucroMensalBruto = ativo.DuracaoMeses > 0 ? lucroTotalBruto / ativo.DuracaoMeses : 0;

                foreach (var mesAno in meses)
                {
                    double impostoMensal = lucroMensalBruto * (impostoPerc / 100);

                    relatorio.Add(new RelatorioImpostosViewModel
                    {
                        Nome = ativo.Nome,
                        TipoAtivo = ativo.TipoAtivo ?? string.Empty,
                        DataReferencia = mesAno.ToDateTime(TimeOnly.MinValue),
                        ImpostoMensal = impostoMensal
                    });
                }
            }

            return View(relatorio);
        }

        private List<DateOnly> GetMesesEntreDatas(DateOnly start, DateOnly end)
        {
            var meses = new List<DateOnly>();
            var current = new DateOnly(start.Year, start.Month, 1);

            while (current <= end)
            {
                meses.Add(current);
                if (current.Month == 12)
                    current = new DateOnly(current.Year + 1, 1, 1);
                else
                    current = new DateOnly(current.Year, current.Month + 1, 1);
            }

            return meses;
        }

        [Authorize(Policy = "AdministradorPolicy")]
        public IActionResult RelatorioAdmin(DateOnly? dataInicio, DateOnly? dataFim)
        {
            if (dataInicio == null || dataFim == null)
            {
                ViewBag.Mensagem = "Por favor, selecione um intervalo de datas.";
                return View(new List<RelatorioAdminViewModel>());
            }
            
            var depositos = _context.DepositoPrazos
                .Include(d => d.AtivoUu)
                .Where(d =>
                    d.AtivoUu.DataInicio >= dataInicio &&
                    d.AtivoUu.DataInicio <= dataFim)
                .ToList();
            
            var relatorio = depositos
                .GroupBy(d => d.Banco)
                .Select(g =>
                {
                    var valorTotal = g.Sum(d => d.ValorInicial);

                    var custoTotalJuros = g.Sum(d =>
                    {
                        var lucroTotal = d.AtivoUu.LucroTotal ?? 0m;
                        var impostoPerc = d.AtivoUu.ImpostoPerc;
                        var jurosLiquidos = lucroTotal * (1 - impostoPerc / 100);
                        return jurosLiquidos;
                    });

                    return new RelatorioAdminViewModel
                    {
                        Banco = g.Key,
                        ValorTotalDepositado = valorTotal,
                        CustoTotalJuros = custoTotalJuros
                    };
                })
                .ToList();

            return View(relatorio);
        }
        
        public IActionResult ExportarPdfRelatorioLucros(DateTime? dataInicio, DateTime? dataFim)
        {
            if (!dataInicio.HasValue || !dataFim.HasValue)
                return BadRequest("Datas inválidas");

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var dataInicioOnly = DateOnly.FromDateTime(dataInicio.Value);
            var dataFimOnly = DateOnly.FromDateTime(dataFim.Value);

            var ativos = _context.Ativos
                .Where(a => a.UserUuid == userId &&
                            a.DataInicio >= dataInicioOnly &&
                            a.DataInicio <= dataFimOnly)
                .ToList();

            var relatorio = ativos.Select(ativo =>
            {
                double lucroTotalBruto = (double)(ativo.LucroTotal ?? 0m);
                double impostoPerc = (double)(ativo.ImpostoPerc);
                double lucroTotalLiquido = lucroTotalBruto * (1 - impostoPerc / 100);

                double lucroMensalBruto = ativo.DuracaoMeses > 0 ? lucroTotalBruto / ativo.DuracaoMeses : 0;
                double lucroMensalLiquido = ativo.DuracaoMeses > 0 ? lucroTotalLiquido / ativo.DuracaoMeses : 0;

                return new RelatorioLucrosViewModel
                {
                    Nome = ativo.Nome,
                    TipoAtivo = ativo.TipoAtivo ?? string.Empty,
                    DataInicio = ativo.DataInicio.ToDateTime(TimeOnly.MinValue),
                    DuracaoMeses = ativo.DuracaoMeses,
                    LucroTotalBruto = lucroTotalBruto,
                    LucroTotalLiquido = lucroTotalLiquido,
                    LucroMensalBruto = lucroMensalBruto,
                    LucroMensalLiquido = lucroMensalLiquido
                };
            }).ToList();

            var colunas = new[] {
                "Nome", "Tipo Ativo", "Data Início", "Duração (meses)",
                "Lucro Total Bruto", "Lucro Total Líquido",
                "Lucro Mensal Bruto", "Lucro Mensal Líquido"
            };

            byte[] pdf = _pdfReportService.GerarTabelaPdf(
                "Relatório de Lucros",
                colunas,
                relatorio,
                item => new object[] {
                    item.Nome,
                    item.TipoAtivo,
                    item.DataInicio.ToString("dd/MM/yyyy"),
                    item.DuracaoMeses,
                    item.LucroTotalBruto.ToString("C"),
                    item.LucroTotalLiquido.ToString("C"),
                    item.LucroMensalBruto.ToString("C"),
                    item.LucroMensalLiquido.ToString("C")
                },
                dataInicioOnly,
                dataFimOnly
            );

            string nomeArquivo = $"Relatorio_Lucros_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", nomeArquivo);
        }

        public IActionResult ExportarPdfRelatorioImpostos(DateTime? dataInicio, DateTime? dataFim)
        {
            if (!dataInicio.HasValue || !dataFim.HasValue)
                return BadRequest("Datas inválidas");

            var userId = GetUserId();
            if (userId == Guid.Empty)
                return Unauthorized();

            var dataInicioOnly = DateOnly.FromDateTime(dataInicio.Value);
            var dataFimOnly = DateOnly.FromDateTime(dataFim.Value);

            var ativos = _context.Ativos
                .Where(a => a.UserUuid == userId && a.DataInicio <= dataFimOnly)
                .ToList();

            var relatorio = new List<RelatorioImpostosViewModel>();

            foreach (var ativo in ativos)
            {
                var dataAtivoInicio = ativo.DataInicio < dataInicioOnly ? dataInicioOnly : ativo.DataInicio;
                var meses = GetMesesEntreDatas(dataAtivoInicio, dataFimOnly);

                double impostoPerc = (double)(ativo.ImpostoPerc);
                double lucroTotalBruto = (double)(ativo.LucroTotal ?? 0m);
                double lucroMensalBruto = ativo.DuracaoMeses > 0 ? lucroTotalBruto / ativo.DuracaoMeses : 0;

                foreach (var mesAno in meses)
                {
                    double impostoMensal = lucroMensalBruto * (impostoPerc / 100);

                    relatorio.Add(new RelatorioImpostosViewModel
                    {
                        Nome = ativo.Nome,
                        TipoAtivo = ativo.TipoAtivo ?? string.Empty,
                        DataReferencia = mesAno.ToDateTime(TimeOnly.MinValue),
                        ImpostoMensal = impostoMensal
                    });
                }
            }

            var colunas = new[] { "Nome", "Tipo Ativo", "Data Referência", "Imposto Mensal" };

            byte[] pdf = _pdfReportService.GerarTabelaPdf(
                "Relatório de Impostos",
                colunas,
                relatorio,
                item => new object[] {
                    item.Nome,
                    item.TipoAtivo,
                    item.DataReferencia.ToString("MM/yyyy"),
                    item.ImpostoMensal.ToString("C")
                },
                dataInicioOnly,
                dataFimOnly
            );

            string nomeArquivo = $"Relatorio_Impostos_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", nomeArquivo);
        }

        [Authorize(Policy = "AdministradorPolicy")]
        public IActionResult ExportarPdfRelatorioAdmin(DateOnly? dataInicio, DateOnly? dataFim)
        {
            if (dataInicio == null || dataFim == null)
                return BadRequest("Datas inválidas");

            var depositos = _context.DepositoPrazos
                .Include(d => d.AtivoUu)
                .Where(d => d.AtivoUu.DataInicio >= dataInicio &&
                            d.AtivoUu.DataInicio <= dataFim)
                .ToList();

            var relatorio = depositos
                .GroupBy(d => d.Banco)
                .Select(g =>
                {
                    var valorTotal = g.Sum(d => d.ValorInicial);

                    var custoTotalJuros = g.Sum(d =>
                    {
                        var lucroTotal = d.AtivoUu.LucroTotal ?? 0m;
                        var impostoPerc = d.AtivoUu.ImpostoPerc;
                        var jurosLiquidos = lucroTotal * (1 - impostoPerc / 100);
                        return jurosLiquidos;
                    });

                    return new RelatorioAdminViewModel
                    {
                        Banco = g.Key,
                        ValorTotalDepositado = valorTotal,
                        CustoTotalJuros = custoTotalJuros
                    };
                })
                .ToList();

            var colunas = new[] { "Banco", "Valor Total Depositado", "Custo Total Juros" };

            byte[] pdf = _pdfReportService.GerarTabelaPdf(
                "Relatório Admin",
                colunas,
                relatorio,
                item => new object[] {
                    item.Banco,
                    item.ValorTotalDepositado.ToString("C"),
                    item.CustoTotalJuros.ToString("C")
                },
                dataInicio.Value,
                dataFim.Value
            );

            string nomeArquivo = $"Relatorio_Admin_{dataInicio:yyyyMMdd}_{dataFim:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", nomeArquivo);
        }
        



    }
}
