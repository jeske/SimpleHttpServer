﻿// Copyright (C) 2016 by Barend Erasmus and donated to the public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleHttpServer.Models
{
    public class Route
    {
        #region Properties

        public string Name { get; set; } // descriptive name for debugging
        public string UrlRegex { get; set; }
        public string Method { get; set; }
        public Func<HttpRequest, HttpResponse> Callable { get; set; }

        #endregion
    }
}
