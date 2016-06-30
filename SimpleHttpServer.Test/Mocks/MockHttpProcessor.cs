using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SimpleHttpServer.Models;
using SimpleHttpServer;

namespace SimpleHttpServer.Test.Mocks
{
    class MockHttpProcessor : HttpProcessor
    {
        public HttpResponse Response { get; set; }
        public HttpRequest Request { get; set; }

        public MockHttpProcessor(HttpRequest request, List<Route> routes)
        {
            this.Request = request;

            foreach (var route in routes)
            {
                this.AddRoute(route);
            }
        }

        public MockHttpProcessor(HttpRequest request, Route route)
        {
            this.Request = request;

            this.AddRoute(route);

        }

        protected override Stream GetInputStream(TcpClient tcpClient)
        {
            return GenerateStreamFromString(this.Request.ToString());
        }

        protected override Stream GetOutputStream(TcpClient tcpClient)
        {
            return new MemoryStream();
        }

        protected override HttpResponse RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request)
        {
            this.Response = base.RouteRequest(inputStream, outputStream, request);

            return this.Response;

        }

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
