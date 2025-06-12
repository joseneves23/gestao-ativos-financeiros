using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Tests.UI;

public class UiFlowTests : IDisposable
{
    private readonly IWebDriver driver;
    public const string BaseUrl = "http://localhost:5244";

    public UiFlowTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--ignore-certificate-errors");
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void FluxoCompleto_LoginNavegacaoLogout_DeveFluxoCompleto()
    {
        // 1. Login
        driver.Navigate().GoToUrl(BaseUrl);
        Thread.Sleep(3000);

        driver.FindElement(By.Id("Email")).SendKeys("admin@ativosfinanceiros.com");
        Thread.Sleep(3000); // pausa para mostrar a digitação do email

        driver.FindElement(By.Id("Password")).SendKeys("admin123");
        Thread.Sleep(3000); // pausa para mostrar a digitação da senha

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.Url.ToLower().Contains("home"));
        Thread.Sleep(3000); // pausa para mostrar a página home carregada

        // 2. Navegar para Meus Ativos
        driver.FindElement(By.LinkText("Meus Ativos")).Click();
        wait.Until(d => d.Url.ToLower().Contains("meusativos"));
        Thread.Sleep(3000); // pausa para mostrar a página Meus Ativos carregada
        Assert.Contains("meusativos", driver.Url.ToLower());

        // 3. Tentar filtrar
        var nomeInput = driver.FindElement(By.Id("nome"));
        nomeInput.SendKeys("teste");
        Thread.Sleep(3000); // pausa para mostrar o preenchimento do filtro

        driver.FindElement(By.XPath("//button[text()='Pesquisar']")).Click();

        Thread.Sleep(10000); // pausa maior para visualizar os resultados do filtro

        // 4. Voltar ao Home
        var homeLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("linkHome")));
        homeLink.Click();
        Thread.Sleep(3000); // pausa para mostrar a navegação para home

        // 5. Logout
        driver.FindElement(By.Id("btnLogout")).Click();
        Thread.Sleep(3000); // pausa para mostrar o clique no logout

        wait.Until(d => d.Url.ToLower().Contains(BaseUrl.ToLower()));
        Assert.Contains(BaseUrl.ToLower(), driver.Url.ToLower());

        Thread.Sleep(3000); // pausa final para visualizar a tela de login
    }

    [Fact]
    public void ResponsiveNavbar_RedimensionarJanela_DeveAdaptarMenu()
    {
        FazerLogin();

        // Redimensionar para mobile
        driver.Manage().Window.Size = new System.Drawing.Size(375, 667);
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        
        // Verificar se o botão do menu mobile aparece
        var navbarToggler = wait.Until(d => d.FindElement(By.CssSelector(".navbar-toggler")));
        Assert.True(navbarToggler.Displayed);

        // Clicar no botão para abrir o menu
        navbarToggler.Click();
        Thread.Sleep(1000);

        // Verificar se o menu colapsável está visível
        var navbarCollapse = driver.FindElement(By.Id("navbarNav"));
        wait.Until(d => navbarCollapse.GetAttribute("class").Contains("show") || 
                       navbarCollapse.GetAttribute("class").Contains("collapsing"));

        Thread.Sleep(2000);

        // Voltar ao tamanho normal
        driver.Manage().Window.Maximize();
        Thread.Sleep(1000);
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