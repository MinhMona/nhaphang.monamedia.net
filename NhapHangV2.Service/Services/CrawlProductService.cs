using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NPOI.HPSF;
using NhapHangV2.Interface.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using DocumentFormat.OpenXml.Bibliography;
using OpenQA.Selenium.Support.UI;

namespace NhapHangV2.Service.Services
{
    public class CrawlProductService : ICrawlProductService
    {

        private static ChromeDriver chromeDriver = null;
        private static readonly long time = 1687241455541;
        private static Dictionary<string, string> results = new Dictionary<string, string>();
        public CrawlProductService()
        {
            if (chromeDriver == null)
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("headless");
                chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                chromeDriver = new ChromeDriver(Environment.CurrentDirectory, chromeOptions);
                chromeDriver.Url = "https://user.lovbuy.com/item.php";
                chromeDriver.Navigate();
            }
        }

        private IWebElement  GetWebElement(IWebDriver driver, string requestId)
        {
            try
            {
                IWebElement webElement = driver.FindElement(By.Id("mona_" + requestId));
                return webElement;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CrawlProduct(long id, string web)
        {
            var requestId = Guid.NewGuid().ToString().Replace('-','_');
            chromeDriver.ExecuteScript("var content=null; $.ajax({ url: \"iteminfo.php\", method: 'post', data: { t1:'" + ((id * 2) + (time * 3)) + "', t2: '" + time * 7 + "', t3: '" + web + "' }, error: function () {  }, success: function (data) { $('body').append('<input id=\"mona_"+ requestId + "\" />'); console.log('"+ requestId+"'); $('#mona_"+ requestId +"').val(JSON.stringify(data)); }});");
            WebDriverWait wait = new WebDriverWait(chromeDriver, TimeSpan.FromSeconds(10));
            IWebElement resultElement = wait.Until(e => GetWebElement(e, requestId));
            string result = resultElement.GetDomProperty("value");
            chromeDriver.ExecuteScript("$('#mona_" + requestId + "').remove()");
            return result;
        }
    }
}
