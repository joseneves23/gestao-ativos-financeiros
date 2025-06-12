using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Tests.UI;

public class LoginTests : IDisposable
{
    private readonly IWebDriver driver;
    public const string BaseUrl = "http://localhost:5244";

    public LoginTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--ignore-certificate-errors");
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void Login_ComCredenciaisValidas_DeveRedirecionarParaDashboard()
    {
        driver.Navigate().GoToUrl(BaseUrl);
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Email")).SendKeys("admin@ativosfinanceiros.com");
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Password")).SendKeys("admin123");
        Thread.Sleep(1000);

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.ToLower().Contains("home"));

        Assert.Contains("home", driver.Url.ToLower());

        Thread.Sleep(3000); // pausa para ver dashboard carregada
    }

    [Fact]
    public void Login_ComCredenciaisInvalidas_DeveMostrarMensagemDeErro()
    {
        driver.Navigate().GoToUrl(BaseUrl);
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Email")).SendKeys("usuario_invalido@teste.com");
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Password")).SendKeys("senhaErrada");
        Thread.Sleep(1000);

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
        {
            var element = d.FindElement(By.CssSelector(".validation-summary-errors ul li"));
            return element.Displayed && !string.IsNullOrEmpty(element.Text);
        });

        var errorMessage = driver.FindElement(By.CssSelector(".validation-summary-errors ul li")).Text;
        Assert.Contains("invalid", errorMessage.ToLower());

        Thread.Sleep(3000); // pausa para ver mensagem de erro
    }

    [Fact]
    public void Logout_DeveRedirecionarParaLogin()
    {
        driver.Navigate().GoToUrl(BaseUrl);
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Email")).SendKeys("admin@ativosfinanceiros.com");
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Password")).SendKeys("admin123");
        Thread.Sleep(1000);

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        Thread.Sleep(3000);

        driver.FindElement(By.Id("btnLogout")).Click();
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.ToLower().Contains(BaseUrl));

        Assert.Contains(BaseUrl, driver.Url.ToLower());
        Thread.Sleep(3000); // pausa para ver a página login após logout
    }

    public void Dispose()
    {
        driver.Quit();
    }
}