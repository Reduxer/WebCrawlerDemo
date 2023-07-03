using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebScrapingDemo
{
    public static class ScrapingUtil
    {

        public static string GetValueByLabel(IWebDriver driver, string xpath, Func<string, string> valueSelector, string defaultValue = "")
        {
            try
            {
                Console.WriteLine("Scraping");
                var node = driver.FindElement(By.XPath(xpath))!;
                return valueSelector.Invoke(node.Text);

            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
