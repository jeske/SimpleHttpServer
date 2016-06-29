using SimpleHttpServer;
using SimpleHttpServer.Models;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleHttpServer
{

    public class HttpServer
    {

        protected int port;
        TcpListener listener;
        bool is_active = true;

        public HttpServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            HttpProcessor p = new HttpProcessor();
            p.AddRoute(new Route()
            {
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        Content = "hello world",
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                },
                Method = "GET",
                Url = "/"
            });

            listener = new TcpListener(port);
            listener.Start();
            while (is_active)
            {
                TcpClient s = listener.AcceptTcpClient();
                Thread thread = new Thread(() =>
                {
                   
                    p.Handle(s);

                });
                thread.Start();
                Thread.Sleep(1);
            }
        }

    }


    public class TestMain
    {
        public static int Main(String[] args)
        {
            HttpServer httpServer;
            if (args.GetLength(0) > 0)
            {
                httpServer = new HttpServer(Convert.ToInt16(args[0]));
            }
            else
            {
                httpServer = new HttpServer(8080);
            }
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            return 0;
        }

    }

}



