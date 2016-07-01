// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using log4net;
using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleHttpServer
{
    public class HttpProcessor
    {

        #region Fields

        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        private List<Route> Routes = new List<Route>();

        private static readonly ILog log = LogManager.GetLogger(typeof(HttpProcessor));

        #endregion

        #region Constructors

        public HttpProcessor()
        {
        }

        #endregion

        #region Public Methods
        public void HandleClient(TcpClient tcpClient)
        {
            try
            {
                log.Info(string.Format("{0} has connected", ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()));

                Stream inputStream = GetInputStream(tcpClient);
                Stream outputStream = GetOutputStream(tcpClient);

                HttpRequest request = GetRequest(inputStream, outputStream);

                // route and handle the request...
                HttpResponse response = RouteRequest(inputStream, outputStream, request);

                WriteResponse(outputStream, response);

                outputStream.Flush();
                outputStream.Close();
                outputStream = null;

                inputStream.Close();
                inputStream = null;

                log.Info(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));

            }catch(Exception ex)
            {
                ExceptionHandler.Handle(log, ex);
            }

        }

        // this formats the HTTP response...
        private static void WriteResponse(Stream stream, HttpResponse response)
        {
            // default to text/html content type
            if (!response.Headers.ContainsKey("Content-Type"))
            {
                response.Headers["Content-Type"] = "text/html";
            }

            response.Headers["Content-Length"] = response.ContentStream.Length.ToString();

            Write(stream, response.ToHeader());

            long totalBytes = response.ContentStream.Length;
            long bytesLeft = totalBytes;

            while (bytesLeft > 0)
            {
                byte[] buffer = new byte[bytesLeft > ConfigurationDefaults.BufferSize ? ConfigurationDefaults.BufferSize : bytesLeft];
                int n = response.ContentStream.Read(buffer, 0, buffer.Length);

                stream.Write(buffer, 0, n);

                bytesLeft -= n;
            }

        }

        public void AddRoute(Route route)
        {
            this.Routes.Add(route);
        }

        #endregion

        #region Private Methods

        private static string Readline(Stream stream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        private static void Write(Stream stream, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual HttpResponse RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request)
        {

            List<Route> routes = this.Routes.Where(x => Regex.Match(request.Url, x.UrlRegex).Success).ToList();

            if (!routes.Any())
                return HttpBuilder.NotFound();

            Route route = routes.SingleOrDefault(x => x.Method == request.Method);

            if (route == null)
                return HttpBuilder.MethodNotAllowed();

            // trigger the route handler...
            request.Route = route;
            try
            {
                var result = route.Callable(request);

                if (result.IsValid())
                {
                    return result;
                }

                return HttpBuilder.InternalServerError();
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return HttpBuilder.InternalServerError();
            }

        }

        private HttpRequest GetRequest(Stream inputStream, Stream outputStream)
        {
            //Read Request Line
            string request = Readline(inputStream);

            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            string method = tokens[0].ToUpper();
            string url = tokens[1];
            string protocolVersion = tokens[2];

            //Read Headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            string line;
            while ((line = Readline(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    break;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                headers.Add(name, value);
            }

            Stream contentStream = new MemoryStream();

            if (headers.ContainsKey("Content-Length"))
            {
                long totalBytes = Convert.ToInt32(headers["Content-Length"]);
                long bytesLeft = totalBytes;

                while (bytesLeft > 0)
                {
                    byte[] buffer = new byte[bytesLeft > ConfigurationDefaults.BufferSize ? ConfigurationDefaults.BufferSize : bytesLeft];
                    int n = inputStream.Read(buffer, 0, buffer.Length);

                    contentStream.Write(buffer, 0, n);

                    bytesLeft -= n;
                }

            }


            return new HttpRequest()
            {
                Method = method,
                Url = url,
                Headers = headers,
                ContentStream = contentStream
            };
        }

        #endregion


    }
}
