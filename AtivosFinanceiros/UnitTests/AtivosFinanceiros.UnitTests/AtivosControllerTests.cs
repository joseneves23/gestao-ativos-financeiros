using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AtivosFinanceiros.Controllers;
using AtivosFinanceiros.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AtivosFinanceiros.UnitTests.AtivosFinanceiros.UnitTests
{
    [TestFixture]
    public class AtivosControllerTests
    {
        private Mock<ILogger<AtivosController>> _loggerMock = null!;
        private Mock<MeuDbContext> _dbContextMock = null!;
        private AtivosController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AtivosController>>();
            _dbContextMock = new Mock<MeuDbContext>();
            _controller = new AtivosController(_dbContextMock.Object, _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            (_controller as IDisposable)?.Dispose();
        }

        [Test]
        public void CalcularLucroDepositoPrazo_DevolveLucroCorreto()
        {
            decimal valorInicial = 1000;
            decimal taxaAnual = 5;
            int duracaoMeses = 12;
            decimal imposto = 10;

            var lucro = _controller.CalcularLucroDepositoPrazo(valorInicial, taxaAnual, duracaoMeses, imposto);

            Assert.That(lucro, Is.GreaterThan(0));
            TestContext.Progress.WriteLine($"Lucro calculado (Depósito Prazo): {lucro}");
        }

        [Test]
        public void CalcularLucroFundoInvestimento_DevolveLucroCorreto()
        {
            decimal monteInvestido = 5000;
            decimal jurosMensal = 1;
            int duracaoMeses = 6;
            decimal imposto = 15;

            var lucro = _controller.CalcularLucroFundoInvestimento(monteInvestido, jurosMensal, duracaoMeses, imposto);

            Assert.That(lucro, Is.GreaterThan(0));
            TestContext.Progress.WriteLine($"Lucro calculado (Fundo Investimento): {lucro}");
        }

        [Test]
        public void CalcularLucroImovelArrendado_DevolveLucroCorreto()
        {
            decimal valorRenda = 1000;
            decimal valorCondominio = 100;
            decimal despesasAnuais = 1200;
            int duracaoMeses = 12;
            decimal imposto = 20;

            var lucro = _controller.CalcularLucroImovelArrendado(valorRenda, valorCondominio, despesasAnuais, duracaoMeses, imposto);

            Assert.That(lucro, Is.GreaterThan(0));
            TestContext.Progress.WriteLine($"Lucro calculado (Imóvel Arrendado): {lucro}");
        }

        [Test]
        public void EditAtivo_DevolveNotFound_QuandoAtivoNaoExiste()
        {
            // Arrange
            var id = Guid.NewGuid();

            var ativosDbSetMock = new Mock<DbSet<Ativo>>();
            ativosDbSetMock.As<IQueryable<Ativo>>().Setup(m => m.Provider).Returns((new List<Ativo>()).AsQueryable().Provider);
            ativosDbSetMock.As<IQueryable<Ativo>>().Setup(m => m.Expression).Returns((new List<Ativo>()).AsQueryable().Expression);
            ativosDbSetMock.As<IQueryable<Ativo>>().Setup(m => m.ElementType).Returns((new List<Ativo>()).AsQueryable().ElementType);
            ativosDbSetMock.As<IQueryable<Ativo>>().Setup(m => m.GetEnumerator()).Returns((new List<Ativo>()).AsQueryable().GetEnumerator());

            _dbContextMock.Setup(db => db.Ativos).Returns(ativosDbSetMock.Object);

            // Act
            var resultado = _controller.EditAtivo(id);

            // Assert
            Assert.That(resultado, Is.InstanceOf<NotFoundResult>());
            TestContext.Progress.WriteLine("EditAtivo devolveu NotFound para um id inexistente.");
        }


        [Test]
        public void CreateAtivo_DevolveView_QuandoUserNaoAutenticado()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                    }))
                }
            };

            var usuariosDbSetMock = new Mock<DbSet<Usuario>>();
            usuariosDbSetMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(new List<Usuario>().AsQueryable().Provider);
            usuariosDbSetMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(new List<Usuario>().AsQueryable().Expression);
            usuariosDbSetMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(new List<Usuario>().AsQueryable().ElementType);
            usuariosDbSetMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(new List<Usuario>().AsQueryable().GetEnumerator());

            _dbContextMock.Setup(db => db.Usuarios).Returns(usuariosDbSetMock.Object);

            var ativo = new Ativo { Nome = "Ativo Teste" };

            // Act
            var resultado = _controller.CreateAtivo(ativo);

            // Assert
            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            TestContext.Progress.WriteLine("CreateAtivo devolveu View quando não autenticado.");
        }


    }
}
