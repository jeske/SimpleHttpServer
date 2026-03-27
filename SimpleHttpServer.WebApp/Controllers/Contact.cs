using SimpleHttpServer.Extensions;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.WebApp.Controllers
{
    class Contact
    {

        public HttpResponse SendEmail(HttpRequest request)
        {
            

            return HttpBuilder.MovedPermanently("/");

        }
    }
}
