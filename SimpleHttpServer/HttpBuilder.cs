using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    class HttpBuilder
    {
        #region Public Methods

        public static HttpResponse InternalServerError()
        {
            string content = File.ReadAllText("Resources/Pages/500.html");

            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                ContentAsUTF8 = content
            };
        }

        public static HttpResponse NotFound()
        {
            string content = File.ReadAllText("Resources/Pages/404.html");

            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.NotFound,
                ContentAsUTF8 = content
            };
        }

        public static HttpResponse MethodNotAllowed()
        {
            string content = File.ReadAllText("Resources/Pages/405.html");

            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
                ContentAsUTF8 = content
            };
        }

        #endregion
    }
}
