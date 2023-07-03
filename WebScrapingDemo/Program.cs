using CsvHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

namespace WebScrapingDemo
{
    public class Program
    {
        static void Main()
        {
            List<SiteInformation> siteInformationList = new();
            var siteUri = new Uri("https://gastateparks.reserveamerica.com/camping/indian-springs-state-park/r/campsiteSearch.do?search=site&page=siteresult&contractCode=GA&parkId=530170");

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless=new");

            IWebDriver driver = new ChromeDriver(chromeOptions);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Navigate().GoToUrl(siteUri);

            bool hasNextPage;

            do
            {
                var campSites = driver.FindElements(By.XPath("//div[@id='shoppingitems']/div[@class='br']"));

                foreach (var site in campSites)
                {
                    Console.WriteLine("site");
                    siteInformationList.Add(ParseSiteInfo(site));
                }

                var nextBtn = driver.FindElement(By.Id("resultNext_top"));
                var nextBtnClass = nextBtn.GetAttribute("class");
                hasNextPage = !nextBtnClass.Contains("disabled");

                if (hasNextPage)
                {
                    nextBtn.Click();
                }

            } while (hasNextPage);

            SaveAsCsv(siteInformationList, siteUri.Host);

            Console.ReadLine();
        }

        private static void SaveAsCsv(List<SiteInformation> siteInformationList, string filename)
        {
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), $"{filename}.csv");

            using var fileWriter = File.CreateText(savePath);
            using var csvWriter = new CsvWriter(fileWriter, CultureInfo.InvariantCulture);
            
            csvWriter.WriteRecords(siteInformationList);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Data was saved in {savePath}");
        }

        private static SiteInformation ParseSiteInfo(IWebElement site)
        {
            var siteInfo = new SiteInformation();
            var siteColumns = site.FindElements(By.XPath("./div[contains(@class, 'td')]"));

            siteInfo.SiteNumber = siteColumns[0].FindElement(By.XPath("./div[@class='siteListLabel']/a")).Text;

            siteInfo.SiteType = siteColumns[2].Text;
            siteInfo.MaxOccupants = int.Parse(siteColumns[3].Text);

            var enterDateBtn = siteColumns[6].FindElement(By.XPath("./a[@class='book now']"));
            var linkUrl = enterDateBtn.GetAttribute("href");

            var siteDetailDriverOption = new ChromeOptions();
            siteDetailDriverOption.AddArgument("--headless=new");
            using var siteDetailDriver = new ChromeDriver(siteDetailDriverOption);

            siteDetailDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            siteDetailDriver.Navigate().GoToUrl(linkUrl);

            siteInfo.Access = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Driveway Entry:')]/strong", (text) =>
            {
                return text ?? "";
            }, "");

            var checkInTime = siteDetailDriver.FindElement(By.XPath("//*[contains(text(), 'Checkin Time:')]/strong")).Text;
            var checkOutTime = siteDetailDriver.FindElement(By.XPath("//*[contains(text(), 'Checkout Time:')]/strong")).Text;

            var inDt = DateTime.Parse(checkInTime);
            var outDt = DateTime.Parse(checkOutTime);

            if(inDt.Hour >= outDt.Hour)
            {
                outDt = outDt.AddDays(1);
            }

            siteInfo.RentalPeriod = (outDt - inDt).TotalHours;

            siteInfo.WaterHookup = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Water Hookup:')]/strong", (text) =>
            {
                return text.Contains('Y', StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
            }, "");

            siteInfo.ElectricHookup = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Electricity Hookup:')]/strong", (text) =>
            {
                return text + "A";
            }, "");

            siteInfo.SewerHookup = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Sewer Hookup:')]/strong", (text) =>
            {
                return text.Contains('Y', StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
            }, "");

            siteInfo.PicnicTable = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Picnic Table:')]/strong", (text) =>
            {
                return text.Contains('Y', StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";
            }, "");

            siteInfo.SiteLength = double.Parse(ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Site Length:')]/strong", (text) =>
            {
                return text;
            }, "0"));

            siteInfo.SiteWidth = double.Parse(ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Site Width:')]/strong", (text) =>
            {
                return text;
            }, "0"));

            var notesElements = siteDetailDriver.FindElements(By.XPath("//*[contains(@class, 'content campsiteNotes')]/li"));
            siteInfo.Notes = string.Join(" | ", notesElements.Select(el => el.Text));

            siteInfo.MaxRVLength = double.Parse(ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Max Vehicle Length:')]/strong", (text) =>
            {
                return text;
            }, "0"));

            siteInfo.SiteSurface = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Driveway Surface:')]/strong", (text) =>
            {
                return text;
            }, "");

            siteInfo.PetsAllowed = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Maximum Number of Pets:')]/strong", (text) =>
            {
                return int.Parse(text) > 0 ? "Yes" : "No";
            }, "");

            siteInfo.AirConditioning = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Air Conditioned/Heated:')]/strong", (text) =>
            {
                return int.Parse(text) > 0 ? "Yes" : "No";
            }, "");

            siteInfo.Kitchen = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Dining Hall/Kitchen:')]/strong", (text) =>
            {
                return int.Parse(text) > 0 ? "Yes" : "No";
            }, "");

            siteInfo.Internet = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'WiFi Access:')]/strong", (text) =>
            {
                return int.Parse(text) > 0 ? "Yes" : "No";
            }, "");

            siteInfo.CableTV = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'TV:')]/strong", (text) =>
            {
                return int.Parse(text) > 0 ? "Yes" : "No";
            }, "");

            siteInfo.Electricity = ScrapingUtil.GetValueByLabel(siteDetailDriver, "//*[contains(text(), 'Electricity Available:')]/strong", (text) =>
            {
                return int.Parse(text) > 0 ? "Yes" : "No";
            }, "");


            // No data available
            siteInfo.PadLength = 0;
            siteInfo.PadWidth = 0;
            siteInfo.FirePit = "";
            siteInfo.Patio = new();
            siteInfo.TentsAllowed = "";
            siteInfo.Heat = "";
            siteInfo.Microwave = "";
            siteInfo.Oven = "";
            siteInfo.PrivateBathroom = "";
            siteInfo.PrivateShower = "";
            siteInfo.Refrigerator = "";
            siteInfo.Stove = "";
            siteInfo.MiniFridge = "";
            siteInfo.Covered = "";
            siteInfo.Ramada = "";
            siteInfo.SiteType = "";

            siteDetailDriver.Quit();

            return siteInfo;
        }
    } 
}