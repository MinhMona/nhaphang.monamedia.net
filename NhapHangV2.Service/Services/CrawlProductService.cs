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
using Newtonsoft.Json.Linq;

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
                //chromeOptions.AddArgument("headless");
                chromeOptions.PageLoadStrategy = PageLoadStrategy.Eager;
                chromeDriver = new ChromeDriver(Environment.CurrentDirectory, chromeOptions);
                chromeDriver.Url = "https://user.lovbuy.com/item.php";
                chromeDriver.Navigate();
                chromeDriver.ExecuteScript("const element= document.createElement(\"script\");  element.append(\"function RemoveInput(id) {  document.getElementById('mona_'+id).remove();}\"); document.body.append(element);");
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
            List<string> requestIdList = new List<string>();
            string result = "";
            for (int i = 0; i < 5; i++)
            {
                requestIdList.Add(Guid.NewGuid().ToString().Replace('-', '_'));
            }
            foreach (var requestId in requestIdList)
            {
                chromeDriver.ExecuteScript("var content=null; $.ajax({ url: \"iteminfo.php\", method: 'post', data: { t1:'" + ((id * 2) + (time * 3)) + "', t2: '" + time * 7 + "', t3: '" + web + "' }, error: function () {  }, success: function (data) { $('body').append('<input id=\"mona_" + requestId + "\" />'); console.log('" + requestId + "'); $('#mona_" + requestId + "').val(JSON.stringify(data));}});");

            }
            foreach (var requestId in requestIdList)
            {
                chromeDriver.ExecuteScript("setTimeout(()=>RemoveInput('" + requestId + "'), 60000);");
            }
            foreach (var requestId in requestIdList)
            {
                WebDriverWait wait = new WebDriverWait(chromeDriver, TimeSpan.FromSeconds(30));
                IWebElement resultElement = wait.Until(e => GetWebElement(e, requestId));
                string data = resultElement.GetDomProperty("value");
                var json = JObject.Parse(data);
                var atributesCheck = json["item"]["props_name"].ToString().Split(';');
                bool isCheck = true;
                if (!string.IsNullOrEmpty(json["item"]["props_name"].ToString()))
                {
                    foreach (var item in atributesCheck)
                    {
                        try
                        {
                            var itemCheck = item.Split(':');
                            if (string.IsNullOrEmpty(itemCheck[3]))
                            {
                                isCheck = false;
                                break;
                            }
                        }
                        catch
                        {
                            isCheck = false;
                            break;
                        }
                    }
                }
                if (isCheck)
                {
                    result = data;
                    break;
                }
            }
            return result;
        }
    }
}
