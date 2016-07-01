using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.Extensions
{
    public static class OtherExtensions
    {
        public static string ToHttpHeaders(this Dictionary<string, string> headers)
        {
            return string.Join("\r\n", headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value)));
        }
    }
}
