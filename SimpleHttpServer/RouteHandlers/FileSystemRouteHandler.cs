using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public string BasePath { get; set; }

        public HttpResponse Handle(HttpRequest request)
        {
            return new HttpResponse()
            {
                Content = File.ReadAllText(string.Format("{0}{1}", this.BasePath, request.Url)),
                ReasonPhrase = "Ok",
                StatusCode = "200"
            };
        }
    }
}
