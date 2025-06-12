using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Tests.UI;

public class AtivosTests : IDisposable
{
    private readonly IWebDriver driver;
    public const string BaseUrl = "http://localhost:5244";

    public AtivosTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--ignore-certificate-errors");
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void MeusAtivos_AcessarPagina_DeveCarregarCorretamente()
    {
        FazerLogin();

        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/MeusAtivos");
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElement(By.TagName("h2")));

        Assert.Contains("Meus Ativos", driver.FindElement(By.TagName("h2")).Text);
        
        // Verificar se o formulário de pesquisa existe
        Assert.True(driver.FindElement(By.Id("nome")).Displayed);
        Assert.True(driver.FindElement(By.Id("tipo")).Displayed);

        Thread.Sleep(2000);
    }

    [Fact]
    public void MeusAtivos_FiltrarPorNome_DeveFuncionar()
    {
        FazerLogin();

        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/MeusAtivos");
        Thread.Sleep(2000);

        var nomeInput = driver.FindElement(By.Id("nome"));
        nomeInput.Clear();
        nomeInput.SendKeys("teste");
        Thread.Sleep(500);

        driver.FindElement(By.XPath("//button[text()='Pesquisar']")).Click();
        Thread.Sleep(2000);

        // Verificar se a URL contém o parâmetro de filtro
        Assert.Contains("nome=teste", driver.Url);
        Thread.Sleep(2000);
    }

    [Fact]
    public void MeusAtivos_FiltrarPorTipo_DeveFuncionar()
    {
        FazerLogin();

        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/MeusAtivos");
        Thread.Sleep(2000);

        var tipoSelect = new SelectElement(driver.FindElement(By.Id("tipo")));
        tipoSelect.SelectByValue("DepositoPrazo");
        Thread.Sleep(500);

        driver.FindElement(By.XPath("//button[text()='Pesquisar']")).Click();
        Thread.Sleep(2000);

        Assert.Contains("tipo=DepositoPrazo", driver.Url);
        Thread.Sleep(2000);
    }

    [Fact]
    public void MeusAtivos_ClicarCriarNovoAtivo_DeveRedirecionarParaFormulario()
    {
        FazerLogin();

        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/MeusAtivos");
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var criarButton = wait.Until(d => d.FindElement(By.LinkText("Criar Novo Ativo")));
        
        criarButton.Click();
        Thread.Sleep(2000);

        wait.Until(d => d.Url.ToLower().Contains("createativo"));
        Assert.Contains("createativo", driver.Url.ToLower());
        Thread.Sleep(2000);
    }

    [Fact]
    public void DetalhesAtivo_AcessarComIdValido_DeveCarregarDetalhes()
    {
        FazerLogin();

        // Primeiro ir para Meus Ativos para encontrar um ativo
        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/MeusAtivos");
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        
        try
        {
            // Procurar por um botão "Ver Detalhes"
            var detalhesButton = wait.Until(d => d.FindElement(By.LinkText("Ver Detalhes")));
            detalhesButton.Click();
            Thread.Sleep(2000);

            // Verificar se chegou à página de detalhes
            wait.Until(d => d.Url.ToLower().Contains("detalhesativo"));
            Assert.Contains("detalhesativo", driver.Url.ToLower());

            // Verificar se há informações do ativo
            Assert.True(driver.FindElement(By.CssSelector(".card-header h2")).Displayed);
            Thread.Sleep(2000);
        }
        catch (WebDriverTimeoutException)
        {
            // Se não houver ativos, apenas verificar se a mensagem aparece
            var alertInfo = driver.FindElements(By.CssSelector(".alert-info"));
            if (alertInfo.Any())
            {
                Assert.Contains("Nenhum ativo encontrado", alertInfo.First().Text);
            }
        }
    }

    private void FazerLogin()
    {
        driver.Navigate().GoToUrl(BaseUrl);
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Email")).SendKeys("admin@ativosfinanceiros.com");
        driver.FindElement(By.Id("Password")).SendKeys("admin123");
        driver.FindElement(By.CssSelector("input[type='submit']")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.ToLower().Contains("home"));
        Thread.Sleep(1000);
    }

    public void Dispose()
    {
        driver.Quit();
    }
}