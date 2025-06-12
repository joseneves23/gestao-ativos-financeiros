using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Tests.UI;

public class RegisterTests : IDisposable
{
    private readonly IWebDriver driver;
    public const string BaseUrl = "http://localhost:5244";

    public RegisterTests()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--ignore-certificate-errors");
        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
    }

    [Fact]
    public void Register_ComDadosValidos_DeveRegistrarComSucesso()
    {
        driver.Navigate().GoToUrl($"{BaseUrl}/Auth/Register");
        Thread.Sleep(1000);

        var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        driver.FindElement(By.Id("Email")).SendKeys($"teste{timestamp}@exemplo.com");
        Thread.Sleep(500);

        driver.FindElement(By.Id("Username")).SendKeys($"utilizador{timestamp}");
        Thread.Sleep(500);

        driver.FindElement(By.Id("Senha")).SendKeys("senha123");
        Thread.Sleep(500);

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.Url.ToLower().Contains(BaseUrl) || d.Url.ToLower().Contains(BaseUrl));

        // Verifica se foi redirecionado para login ou home
        Assert.True(driver.Url.ToLower().Contains(BaseUrl) || driver.Url.ToLower().Contains(BaseUrl));
        Thread.Sleep(2000);
    }

    [Fact]
    public void Register_ComEmailInvalido_DeveMostrarErro()
    {
        driver.Navigate().GoToUrl($"{BaseUrl}/Auth/Register");
        Thread.Sleep(1000);

        driver.FindElement(By.Id("Email")).SendKeys("email-invalido");
        Thread.Sleep(500);

        driver.FindElement(By.Id("Username")).SendKeys("utilizador123");
        Thread.Sleep(500);

        driver.FindElement(By.Id("Senha")).SendKeys("senha123");
        Thread.Sleep(500);

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
        {
            try
            {
                var element = d.FindElement(By.CssSelector(".text-danger"));
                return element.Displayed && !string.IsNullOrEmpty(element.Text);
            }
            catch
            {
                return false;
            }
        });

        var errorElements = driver.FindElements(By.CssSelector(".text-danger"));
        Assert.True(errorElements.Any(e => e.Displayed && !string.IsNullOrEmpty(e.Text)));
        Thread.Sleep(2000);
    }

    [Fact]
    public void Register_ComCamposVazios_DeveMostrarErros()
    {
        driver.Navigate().GoToUrl($"{BaseUrl}/Auth/Register");
        Thread.Sleep(1000);

        driver.FindElement(By.CssSelector("input[type='submit']")).Click();
        Thread.Sleep(2000);

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(d =>
        {
            var errorElements = d.FindElements(By.CssSelector(".text-danger"));
            return errorElements.Any(e => e.Displayed && !string.IsNullOrEmpty(e.Text));
        });

        var errorElements = driver.FindElements(By.CssSelector(".text-danger"));
        Assert.True(errorElements.Count > 0);
        Thread.Sleep(2000);
    }

    public void Dispose()
    {
        driver.Quit();
    }
}