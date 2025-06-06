using System;
using System.Linq;
using AtivosFinanceiros.Controllers;
using AtivosFinanceiros.Models;
using AtivosFinanceiros.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace AtivosFinanceiros.UnitTests.AtivosFinanceiros.UnitTests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<ILogger<AuthController>> _loggerMock;
        private Mock<MeuDbContext> _dbContextMock;
        private Mock<AuthService> _authServiceMock;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            TestContext.Progress.WriteLine("Configuração inicial: a preparar os mocks e o controlador.");
            _loggerMock = new Mock<ILogger<AuthController>>();
            _dbContextMock = new Mock<MeuDbContext>();
            _authServiceMock = new Mock<AuthService>(_dbContextMock.Object, new Mock<ILogger<AuthService>>().Object);

            _controller = new AuthController(_loggerMock.Object, _dbContextMock.Object, _authServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.Progress.WriteLine("Limpeza final: a eliminar o controlador.");
            _controller?.Dispose();
        }

        [Test]
        public void Register_Post_DevolveView_QuandoModelStateInvalido()
        {
            TestContext.Progress.WriteLine("Teste: Register_Post_DevolveView_QuandoModelStateInvalido.");

            // Arrange
            _controller.ModelState.AddModelError("Email", "Obrigatório");
            TestContext.Progress.WriteLine("ModelState configurado com erro no Email.");

            // Act
            var resultado = _controller.Register(new Usuario());
            TestContext.Progress.WriteLine($"Resultado: {resultado.GetType().Name}");

            // Assert
            Assert.That(resultado, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Register_Post_DevolveView_QuandoEmailJaExiste()
        {
            TestContext.Progress.WriteLine("Teste: Register_Post_DevolveView_QuandoEmailJaExiste.");

            // Arrange
            var utilizador = new Usuario { Email = "teste@teste.com" };
            var utilizadorExistente = new Usuario { Email = "teste@teste.com" };

            var utilizadoresDbSet = new[] { utilizadorExistente }.AsQueryable();

            var utilizadoresMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Usuario>>();
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(utilizadoresDbSet.Provider);
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(utilizadoresDbSet.Expression);
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(utilizadoresDbSet.ElementType);
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(utilizadoresDbSet.GetEnumerator());

            _dbContextMock.Setup(x => x.Usuarios).Returns(utilizadoresMock.Object);
            TestContext.Progress.WriteLine("Mock do DbSet configurado com utilizador existente.");

            // Act
            var resultado = _controller.Register(utilizador);
            TestContext.Progress.WriteLine($"Resultado: {resultado.GetType().Name}");

            // Assert
            Assert.That(resultado, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ModelState["Email"].Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void Register_Post_RedirecionaParaLogin_QuandoRegistoBemSucedido()
        {
            TestContext.Progress.WriteLine("Teste: Register_Post_RedirecionaParaLogin_QuandoRegistoBemSucedido.");

            // Arrange
            var utilizador = new Usuario { Email = "novo@teste.com", Senha = "password", Username = "NovoUtilizador" };

            var utilizadoresMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Usuario>>();
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns((new Usuario[] { }).AsQueryable().Provider);
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns((new Usuario[] { }).AsQueryable().Expression);
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns((new Usuario[] { }).AsQueryable().ElementType);
            utilizadoresMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns((new Usuario[] { }).AsQueryable().GetEnumerator());

            _dbContextMock.Setup(x => x.Usuarios).Returns(utilizadoresMock.Object);
            TestContext.Progress.WriteLine("Mock do DbSet configurado sem utilizadores existentes.");

            // Act
            var resultado = _controller.Register(utilizador);
            TestContext.Progress.WriteLine($"Resultado: {resultado.GetType().Name}");

            // Assert
            Assert.That(resultado, Is.InstanceOf<RedirectToActionResult>());
            var resultadoRedirect = resultado as RedirectToActionResult;
            Assert.That(resultadoRedirect.ActionName, Is.EqualTo("Login"));
        }
    }
}
