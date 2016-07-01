// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;
using SimpleHttpServer.WebApp.Controllers;
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
                        Callable = new Home().Index,
                        UrlRegex = "^\\/$",
                        Method = "GET"
                    },
                     new Route()
                    {
                        Callable = new Home().Error,
                        UrlRegex = "^\\/Error$",
                        Method = "GET"
                    },
                    new Route()
                    {
                        Callable = new FileSystemRouteHandler() {
                            BasePath = string.Format(@"{0}\{1}",AppDomain.CurrentDomain.BaseDirectory, "Static"),
                            ShowDirectories = true
                        }.Handle,
                        UrlRegex = "^\\/Static\\/(.*)$",
                        Method = "GET"
                    }
                };

            }
        }
    }
}
