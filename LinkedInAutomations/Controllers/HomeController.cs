using LinkedInAutomations.Models;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RestSharp;
using System.Diagnostics;
using System.Net;

namespace LinkedInAutomations.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/")]
        public IActionResult Index(string email, string password, string ipAddress, string portProxy)
        {
            LogIn(email, password, ipAddress, portProxy);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void LogIn(string email, string password, string ipAddress, string portProxy)
        {
            ChromeOptions options = new ChromeOptions();

            options.AddArgument("headless");

            if (ipAddress is not null || portProxy is not null)
            {
                var proxy = new Proxy
                {
                    Kind = ProxyKind.Manual,
                    IsAutoDetect = false,
                    HttpProxy = $"http://{ipAddress}:{portProxy}"
                };
                options.Proxy = proxy;
            }
            
            IWebDriver driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl("https://www.linkedin.com");

            // Увійти до облікового запису
            driver.FindElement(By.CssSelector("a.nav__button-secondary")).Click();
            driver.FindElement(By.Id("username")).SendKeys(email);
            driver.FindElement(By.Id("password")).SendKeys(password);
            driver.FindElement(By.CssSelector("button.btn__primary--large")).Click();

            System.Threading.Thread.Sleep(3000);

            var profilePhotoUrl = driver.FindElement(By.CssSelector("img.global-nav__me-photo.evi-image.ember-view")).GetAttribute("src");
            var name = driver.FindElement(By.CssSelector("img.global-nav__me-photo.evi-image.ember-view")).GetAttribute("alt");
            SaveProfilePhoto(profilePhotoUrl, name);

            driver.Quit();
        }

        private void SaveProfilePhoto(string url, string name)
        {
            string fileName = $"{name}.jpg";
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(url), fileName);
            }
        }
    }
}
