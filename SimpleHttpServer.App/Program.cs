using SimpleHttpServer;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleHttpServer.App
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer httpServer = new HttpServer(8080, new List<Models.Route>()
            {
                new Route()
                {
                    Url = "/Test/Get",
                    Method = "GET",
                    Callable = (HttpRequest request) =>
                    {
                        return new HttpResponse()
                        {
                            Content = "Hello",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                }


            });
            
            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
