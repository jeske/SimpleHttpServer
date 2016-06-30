using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.WebApp
{
    static class Routes
    {

        public static List<Route> GET
        {
            get
            {
                return new List<Route>()
                {
                    new Route()
                    {
                        Callable = HomeIndex,
                        Url = "^\\/$",
                        Method = "GET"
                    },
                    new Route()
                    {
                        Callable = new FileSystemRouteHandler() { BasePath = @"C:\Users\Barend.Erasmus\Desktop\Test"}.Handle,
                        Url = "^\\/Static\\/(.*)$",
                        Method = "GET"
                    }
                };

            }
        }

        private static HttpResponse HomeIndex(HttpRequest request)
        {
            return new HttpResponse()
            {
                Content = "Hello",
                ReasonPhrase = "OK",
                StatusCode = "200"
            };

        }
    }
}
