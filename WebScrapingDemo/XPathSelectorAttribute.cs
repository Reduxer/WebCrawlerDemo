using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapingDemo
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class XPathSelectorAttribute : Attribute
    {
        public string Value { get; set; } = default!;
    }
}
