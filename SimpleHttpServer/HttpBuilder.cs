﻿using SimpleHttpServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{

    public enum HttpHeader
    {
        Location
    }

    public class HttpBuilder
    {
        #region Public Methods

        public static HttpResponse InternalServerError(Exception ex)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("message", ex.Message);

            string content = File.ReadAllText("Resources/Pages/500.html");

            ViewEngine.SimpleViewEngine viewEngine = new ViewEngine.SimpleViewEngine();
            content = viewEngine.Render(content, data);

            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                ContentAsUTF8 = content
            };
        }

        public static HttpResponse MovedPermanently(string url)
        {
            string content = File.ReadAllText("Resources/Pages/404.html");

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(HttpHeader.Location.ToString(), url);

            return new HttpResponse()
            {
                HttpStatusCode = HttpStatusCode.MovedPermanently,
                ContentAsUTF8 = string.Empty,
                Headers = headers
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
