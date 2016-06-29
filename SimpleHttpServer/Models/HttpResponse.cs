using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleHttpServer.Models
{
    public class HttpResponse
    {
        public string StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
                if (!this.Headers.ContainsKey("Content-Length"))
                    this.Headers.Add("Content-Length", this.Content.Length.ToString());

            return string.Format("HTTP/1.0 {0} {1}\r\n{2}\r\n\r\n{3}", this.StatusCode, this.ReasonPhrase, string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))), this.Content);
        }
    }
}
