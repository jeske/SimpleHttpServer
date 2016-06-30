// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleHttpServer.WebApp
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer httpServer = new HttpServer(8080, Routes.GET);

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
