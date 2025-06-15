using NUnit.Framework;
using AtivosFinanceiros.Services.Reports;
using System;
using System.Collections.Generic;
using QuestPDF.Infrastructure;

namespace AtivosFinanceiros.NUnitTests
{
    public class PdfReportServiceTests
    {
        private PdfReportService _service;

        [SetUp]
        public void Setup()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _service = new PdfReportService();
        }

        public class Dummy
        {
            public string Nome { get; set; }
            public decimal Valor { get; set; }
        }

        [Test]
        public void GerarTabelaPdf_DeveRetornarPdf_Valido()
        {
            var dados = new List<Dummy>
            {
                new Dummy { Nome = "Teste", Valor = 100.50m },
                new Dummy { Nome = "Outro", Valor = 200.75m }
            };

            string[] colunas = { "Nome", "Valor" };
            Func<Dummy, object[]> map = item => new object[] { item.Nome, item.Valor };
            var dataInicio = new DateOnly(2024, 01, 01);
            var dataFim = new DateOnly(2024, 12, 31);

            var resultado = _service.GerarTabelaPdf("Relatório Teste", colunas, dados, map, dataInicio, dataFim);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Length, Is.GreaterThan(0));
            Assert.That(resultado.Length, Is.GreaterThan(1000));
        }
        
        [Test]
        public void GerarTabelaPdf_ComParametrosNulos_DeveLancarArgumentNullException()
        {
            // Teste para colunas nulas
            Assert.Throws<ArgumentNullException>(() =>
                _service.GerarTabelaPdf<Dummy>("Teste", null, new List<Dummy>(), x => new object[0], new DateOnly(), new DateOnly()));

            // Teste para dados nulos
            Assert.Throws<ArgumentNullException>(() =>
                _service.GerarTabelaPdf<Dummy>("Teste", new string[0], null, x => new object[0], new DateOnly(), new DateOnly()));

            // Teste para mapLinha nulo
            Assert.Throws<ArgumentNullException>(() =>
                _service.GerarTabelaPdf<Dummy>("Teste", new string[0], new List<Dummy>(), null, new DateOnly(), new DateOnly()));
        }

        [Test]
        public void GerarTabelaPdf_ComListaVazia_DeveRetornarPdf()
        {
            var dados = new List<Dummy>();
            string[] colunas = { "Nome", "Valor" };
            Func<Dummy, object[]> map = item => new object[] { item.Nome, item.Valor };
            var dataInicio = new DateOnly(2024, 01, 01);
            var dataFim = new DateOnly(2024, 12, 31);

            var resultado = _service.GerarTabelaPdf("Relatório Vazio", colunas, dados, map, dataInicio, dataFim);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Length, Is.GreaterThan(0));
        }
    }
}
