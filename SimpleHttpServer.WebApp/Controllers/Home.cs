using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.WebApp.Controllers
{
    class Home
    {
        public HttpResponse Index(HttpRequest request)
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = File.ReadAllText("Views/Home/Index.html"),
                HttpStatusCode = HttpStatusCode.Ok
            };

        }

        public HttpResponse Error(HttpRequest request)
        {
            throw new Exception("Can't call this action.");

        }
    }
}
