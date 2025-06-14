using System;
using System.Linq;
using System.Collections.Generic;
using AtivosFinanceiros.Models;
using AtivosFinanceiros.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Moq;
using NUnit.Framework;

namespace AtivosFinanceiros.TestesUnit
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<MeuDbContext> _dbContextMock;
        private Mock<ILogger<AuthService>> _loggerMock;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            _dbContextMock = new Mock<MeuDbContext>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(_dbContextMock.Object, _loggerMock.Object);
        }
        
        private Mock<DbSet<T>> CriarDbSetMock<T>(IQueryable<T> dados) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(dados.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(dados.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(dados.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(dados.GetEnumerator());
            return mockSet;
        }

        [Test]
        public void ValidateUser_RetornaUsuario_SeCredenciaisValidas()
        {
            var email = "teste@teste.com";
            var senha = "1234";
            var senhaHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: senha,
                salt: new byte[0],
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            var usuario = new Usuario { Email = email, Senha = senhaHash, Username = "Utilizador1" };

            var dados = new List<Usuario> { usuario }.AsQueryable();
            var usuariosMock = CriarDbSetMock(dados);

            _dbContextMock.Setup(db => db.Usuarios).Returns(usuariosMock.Object);

            var resultado = _authService.ValidateUser(email, senha);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Email, Is.EqualTo(email));
        }

        
        
        
        [Test]
        public void ValidateUser_RetornaNull_SeEmailNaoExiste()
        {
            var usuarios = new Usuario[] { }.AsQueryable();
            var usuariosMock = CriarDbSetMock(usuarios);

            _dbContextMock.Setup(db => db.Usuarios).Returns(usuariosMock.Object);

            var resultado = _authService.ValidateUser("naoexiste@teste.com", "qualquer");

            Assert.That(resultado, Is.Null);
        }
        
        
        

        [Test]
        public void ValidateUser_RetornaNull_SeSenhaErrada()
        {
            var senhaCorreta = "senha123";
            var senhaErrada = "errada";

            var hashedSenha = Convert.ToBase64String(
                Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                    password: senhaCorreta,
                    salt: new byte[0],
                    prf: Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA1,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

            var usuario = new Usuario
            {
                Email = "teste@teste.com",
                Senha = hashedSenha
            };

            var usuarios = new[] { usuario }.AsQueryable();
            var usuariosMock = CriarDbSetMock(usuarios);

            _dbContextMock.Setup(db => db.Usuarios).Returns(usuariosMock.Object);

            var resultado = _authService.ValidateUser(usuario.Email, senhaErrada);

            Assert.That(resultado, Is.Null);
        }
        
        
        
        [Test]
        public void RegisterUser_AdicionaUsuario_ComSenhaHashETipoPerfilUtilizador()
        {
            var usuario = new Usuario
            {
                Email = "novo@teste.com",
                Senha = "minhasenha"
            };

            var usuariosDbSetMock = new Mock<DbSet<Usuario>>();
            _dbContextMock.Setup(db => db.Usuarios).Returns(usuariosDbSetMock.Object);

            _authService.RegisterUser(usuario);

            usuariosDbSetMock.Verify(u => u.Add(It.Is<Usuario>(x =>
                x.Email == usuario.Email &&
                x.Senha != "minhasenha" &&
                x.TipoPerfil == "UTILIZADOR"
            )), Times.Once);

            _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Test]
        public void RegisterUserAdmin_AdicionaUsuario_ComSenhaHashETipoPerfilSemAlterar()
        {
            var usuario = new Usuario
            {
                Email = "admin@teste.com",
                Senha = "adminsenha",
                TipoPerfil = "ADMIN"
            };

            var usuariosDbSetMock = new Mock<DbSet<Usuario>>();
            _dbContextMock.Setup(db => db.Usuarios).Returns(usuariosDbSetMock.Object);

            _authService.RegisterUserAdmin(usuario);

            usuariosDbSetMock.Verify(u => u.Add(It.Is<Usuario>(x =>
                x.Email == usuario.Email &&
                x.Senha != "adminsenha" &&
                x.TipoPerfil == "ADMIN"
            )), Times.Once);

            _dbContextMock.Verify(db => db.SaveChanges(), Times.Once);
        }
    }
}
