using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Tests.UI;

public class NavigationTests : IDisposable
{
    private readonly IWebDriver driver;
    public const string BaseUrl = "http://localhost:5244";

    public NavigationTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--ignore-certificate-errors");
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void Navigation_VerificarMenuPrincipal_DeveConterTodosOsLinks()
    {
        // Primeiro fazer login
        FazerLogin();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElement(By.CssSelector(".navbar")));

        // Verificar se os links do menu existem
        Assert.True(driver.FindElement(By.LinkText("Home")).Displayed);
        Assert.True(driver.FindElement(By.LinkText("Meus Ativos")).Displayed);
        Assert.True(driver.FindElement(By.LinkText("Gerir Relatórios")).Displayed);

        Thread.Sleep(2000);
    }

    [Fact]
    public void Navigation_ClicarEmMeusAtivos_DeveRedirecionarParaPaginaCorreta()
    {
        FazerLogin();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElement(By.LinkText("Meus Ativos")));

        driver.FindElement(By.LinkText("Meus Ativos")).Click();
        Thread.Sleep(2000);

        wait.Until(d => d.Url.ToLower().Contains("meusativos"));
        Assert.Contains("meusativos", driver.Url.ToLower());

        // Verificar se a página contém elementos esperados
        Assert.True(driver.FindElement(By.TagName("h2")).Text.Contains("Meus Ativos"));
        Thread.Sleep(2000);
    }

    [Fact]
    public void Navigation_MenuRelatorios_DeveAbrirDropdown()
    {
        FazerLogin();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        var dropdownToggle = wait.Until(d => d.FindElement(By.CssSelector(".dropdown-toggle")));

        dropdownToggle.Click();
        Thread.Sleep(1000);

        // Verificar se os itens do dropdown estão visíveis
        wait.Until(d => d.FindElement(By.LinkText("Relatório de Impostos")).Displayed);
        Assert.True(driver.FindElement(By.LinkText("Relatório de Impostos")).Displayed);
        Assert.True(driver.FindElement(By.LinkText("Relatório de Lucros")).Displayed);

        Thread.Sleep(2000);
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