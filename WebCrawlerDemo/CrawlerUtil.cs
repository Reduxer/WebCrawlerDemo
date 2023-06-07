using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawlerDemo
{
    public static class CrawlerUtil
    {

        public static string GetValueByLabel(IWebDriver driver, string xpath, Func<string, string> valueSelector, string defaultValue = "")
        {
            try
            {
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
