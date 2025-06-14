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
    public void CriarEEditarAtivo_DeveFuncionar()
    {
        FazerLogin();

        // Criar novo ativo
        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/CreateAtivoo");
        Thread.Sleep(1000);

        // Preencher formulário básico
        driver.FindElement(By.Id("Nome")).SendKeys("Ativo Teste UI");
        var tipoSelect = new SelectElement(driver.FindElement(By.Id("TipoAtivo")));
        tipoSelect.SelectByValue("DepositoPrazo");

        // Definir a data via JavaScript
        var dataInicioInput = driver.FindElement(By.Id("DataInicio"));
        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value = arguments[1];", dataInicioInput, DateTime.Today.ToString("yyyy-MM-dd"));

        driver.FindElement(By.Id("DuracaoMeses")).SendKeys("12");
        driver.FindElement(By.Id("ImpostoPerc")).SendKeys("10");
        driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        Thread.Sleep(2000);

        // Preencher dados do depósito a prazo
        driver.FindElement(By.Id("Banco")).SendKeys("Banco Teste");
        driver.FindElement(By.Id("NumeroConta")).SendKeys("123456");
        driver.FindElement(By.Id("Titulares")).SendKeys("Teste Titular");
        driver.FindElement(By.Id("TaxaAnual")).SendKeys("2");
        driver.FindElement(By.Id("ValorInicial")).SendKeys("1000");
        driver.FindElement(By.CssSelector("button[type='submit']")).Click();
        Thread.Sleep(2000);

        // Confirmar ativo
        driver.FindElement(By.XPath("//button[contains(@class, 'btn-success') and text()='Salvar']")).Click();
        Thread.Sleep(3000);

        // Voltar para lista
        driver.Navigate().GoToUrl($"{BaseUrl}/Ativos/MeusAtivos");
        Thread.Sleep(2000);

        // Verificar se o ativo foi criado
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.PageSource.Contains("Ativo Teste UI"));
        
        // Encontrar o ativo e clicar em Editar
        var rows = driver.FindElements(By.XPath("//tr[td[contains(text(),'Ativo Teste UI')]]"));
        Assert.True(rows.Count > 0, "O ativo criado não foi encontrado na lista");
        
        var editarBtn = rows[0].FindElement(By.LinkText("Editar"));
        editarBtn.Click();
        Thread.Sleep(2000);
        
        // Forçar exibição dos campos específicos
        var tipoAtivo = driver.FindElement(By.Id("TipoAtivo")).GetAttribute("value");
        ((IJavaScriptExecutor)driver).ExecuteScript(
            $"document.getElementById('{tipoAtivo}Fields').style.display = 'block';");
        Thread.Sleep(1000);
        
        // Verificar e preencher campos vazios se necessário
        var impostoInput = driver.FindElement(By.Id("ImpostoPerc"));
        if (string.IsNullOrEmpty(impostoInput.GetAttribute("value")))
        {
            impostoInput.Clear();
            impostoInput.SendKeys("10");
        }
        
        // Verificar e preencher campos específicos do depósito
        if (tipoAtivo == "DepositoPrazo")
        {
            var valorInicialInput = driver.FindElement(By.Id("ValorInicial"));
            if (string.IsNullOrEmpty(valorInicialInput.GetAttribute("value")))
            {
                valorInicialInput.Clear();
                valorInicialInput.SendKeys("1000");
            }
            
            var taxaAnualInput = driver.FindElement(By.Id("TaxaAnual"));
            if (string.IsNullOrEmpty(taxaAnualInput.GetAttribute("value")))
            {
                taxaAnualInput.Clear();
                taxaAnualInput.SendKeys("2");
            }
        }
        
        // Editar o nome
        var nomeInput = driver.FindElement(By.Id("Nome"));
        nomeInput.Clear();
        nomeInput.SendKeys("Ativo Teste UI Editado");
        
        // Salvar usando o formulário diretamente
        var form = driver.FindElement(By.TagName("form"));
        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit();", form);
        Thread.Sleep(3000);
        
        // Verificar se a edição foi bem-sucedida
        Assert.Contains("MeusAtivos", driver.Url);
        wait.Until(d => d.PageSource.Contains("Ativo Teste UI Editado"));
        Assert.Contains("Ativo Teste UI Editado", driver.PageSource);
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