// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

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
                        UrlRegex = "^\\/$",
                        Method = "GET"
                    },
                    new Route()
                    {
                        Callable = new FileSystemRouteHandler() { BasePath = @"C:\Users\Barend.Erasmus\Desktop\Test"}.Handle,
                        UrlRegex = "^\\/Static\\/(.*)$",
                        Method = "GET"
                    }
                };

            }
        }

        private static HttpResponse HomeIndex(HttpRequest request)
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = "Hello",
                ReasonPhrase = "OK",
                StatusCode = "200"
            };

        }
    }
}
