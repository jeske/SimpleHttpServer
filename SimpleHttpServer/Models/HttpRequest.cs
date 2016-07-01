// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using SimpleHttpServer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
        public Stream ContentStream { get; set; }
        public Route Route { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        #endregion

        public string ContentAsUTF8
        {
            set
            {
                ContentStream = value.ToStream();
            }
        }

        #region Constructors
        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods
        public override string ToString()
        {
            return string.Format("{0} - {1}", this.Method, this.Url);
        }

        public string ToHeader()
        {
            if (!this.Headers.ContainsKey("Content-Length"))
                this.Headers.Add("Content-Length", this.ContentStream.Length.ToString());

            return string.Format("{0} {1} HTTP/{2}\r\n{3}\r\n\r\n", this.Method, this.Url, ConfigurationDefaults.HttpVersion, this.Headers.ToHttpHeaders());
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

        #region Private Methods


        #endregion
    }
}
