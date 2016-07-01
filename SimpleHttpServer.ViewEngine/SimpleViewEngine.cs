using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleHttpServer.ViewEngine
{
    public class SimpleViewEngine
    {
        public string Render(string html, Dictionary<string, string> data)
        {

            html = Regex.Replace(html, "{{(?<name>((?!}).)*)}}", x => data[x.Groups["name"].Value]);

            return html;
        }
    }
}
