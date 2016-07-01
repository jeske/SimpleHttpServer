using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
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
                ContentAsUTF8 = "Hello",
                HttpStatusCode = HttpStatusCode.Ok
            };

        }
    }
}
