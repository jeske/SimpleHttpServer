// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using SimpleHttpServer.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// NOTE: two consequences of this simplified response model are:
//
//      (a) it's not possible to send 8-bit clean responses (like file content)
//      (b) it's 
//       must be loaded into memory in the the Content property. If you want to send large files,
//       this has to be reworked so a handler can write to the output stream instead. 

namespace SimpleHttpServer.Models
{
    public enum HttpStatusCode
    {
        // for a full list of status codes, see..
        // https://en.wikipedia.org/wiki/List_of_HTTP_status_codes

        Continue = 100,
        Ok = 200,
        Created = 201,
        Accepted = 202,
        MovedPermanently = 301,
        Found = 302,
        NotModified = 304,
        BadRequest = 400,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        InternalServerError = 500,
        BadGateway = 502,
        ServiceUnavailable = 503

    }

    public class HttpResponse
    {

        #region Properties

        public HttpStatusCode HttpStatusCode { get; set; }
        public Stream ContentStream { get; set; }
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

        public HttpResponse()
        {
            this.Headers = new Dictionary<string, string>();
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return string.Format("{0} {1}", (int)HttpStatusCode, HttpStatusCode.ToString());
        }

        public string ToHeader()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.Append(string.Format("HTTP/1.0 {0} {1}\r\n", (int)HttpStatusCode, HttpStatusCode.ToString()));
            strBuilder.Append(string.Join("\r\n", Headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value))));
            strBuilder.Append("\r\n\r\n");

            return strBuilder.ToString();

        }

        public bool IsValid()
        {
            if (ContentStream == null)
                return false;

            return true;
        }

        #endregion
    }
}
