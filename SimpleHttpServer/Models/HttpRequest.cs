// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleHttpServer.Models
{
    public class HttpRequest
    {
        #region Fields

        #endregion

        #region Properties

        public string Method { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public Route Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Constructors
        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods
        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.Content))
                if (!this.Headers.ContainsKey("Content-Length"))
                    this.Headers.Add("Content-Length", this.Content.Length.ToString());

            return string.Format("{0} {1} HTTP/1.0\r\n{2}\r\n\r\n{3}", this.Method, this.Url, string.Join("\r\n", this.Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))), this.Content);
        }

        public string GetPath()
        {
            var match = Regex.Match(Url, Route.UrlRegex);
            if (match.Groups.Count > 1)
                return match.Groups[1].Value;
            else
                return Url;

        }
        #endregion
    }
}
