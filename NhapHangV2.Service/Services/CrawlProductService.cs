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

namespace NhapHangV2.Service.Services
{
    public class CrawlProductService : ICrawlProductService
    {

        private static ChromeDriver chromeDriver = null;
        private static bool isDone = false;
        private static bool lockDown = false;
        private static readonly long time = 1687241455541;
        private static string result = "";
        public CrawlProductService()
        {
            if (chromeDriver == null)
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("headless");
                chromeDriver = new ChromeDriver("/", chromeOptions);
                INetwork networkInterceptor = chromeDriver.Manage().Network;
                networkInterceptor.NetworkResponseReceived += NetworkInterceptor_NetworkResponseReceived;
                networkInterceptor.StartMonitoring();
                chromeDriver.Url = "https://user.lovbuy.com/item.php";
                chromeDriver.Navigate();
            }
        }

        private static void NetworkInterceptor_NetworkResponseReceived(object sender, NetworkResponseReceivedEventArgs e)
        {
            if (e.ResponseUrl == "https://user.lovbuy.com/iteminfo.php")
            {
                result = e.ResponseBody;
                isDone = true;
            }
        }

        public async Task<string> CrawlProduct(long id, string web)
        {
            while (lockDown)
            {
                Thread.Sleep(100);
            }
            lockDown = true;
            chromeDriver.ExecuteScript("var content=null; $.ajax({ url: \"iteminfo.php\", method: 'post', data: { t1:'" + ((id * 2) + (time * 3)) + "', t2: '" + time * 7 + "', t3: '" + web + "' }, error: function () {  }, success: function (data) {}});");
            while (!isDone)
            {
                Thread.Sleep(100);
            }
            isDone = false;
            lockDown = false;
            return result;
        }
    }
}
