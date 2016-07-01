using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using SimpleHttpServer.Test.Mocks;
using System.Text;
using SimpleHttpServer.Models;

namespace SimpleHttpServer.Test
{
    [TestClass]
    public class HttpProcessorTest
    {
        [TestMethod]
        public void HandleClient_GivenGETRequest_Returns200Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "GET",
                Url = "/Test/Example"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                }
            });

            httpProcessor.HandleClient(null);
            Assert.AreEqual(HttpStatusCode.Ok, httpProcessor.Response.HttpStatusCode);
        }

        [TestMethod]
        public void HandleClient_GivenGETRequestThrowsException_Returns500Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "GET",
                Url = "/Test/Example"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    throw new Exception();
                }
            });

            httpProcessor.HandleClient(null);
            Assert.AreEqual(HttpStatusCode.InternalServerError, httpProcessor.Response.HttpStatusCode);
        }

        [TestMethod]
        public void HandleClient_GivenGETRequestWhereNotRouted_Returns404Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "GET",
                Url = "/Test/Example/NotFound"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                }
            });

            httpProcessor.HandleClient(null);
            Assert.AreEqual(HttpStatusCode.NotFound, httpProcessor.Response.HttpStatusCode);
        }

        [TestMethod]
        public void HandleClient_GivenGETRequestWithQueryString_Returns200Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "GET",
                Url = "/Test/Example?id=10"
            }, new Route()
            {
                UrlRegex = "^\\/Test\\/Example\\?id=(\\d+)$",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                }
            });

            httpProcessor.HandleClient(null);


            Assert.AreEqual(HttpStatusCode.Ok, httpProcessor.Response.HttpStatusCode);
        }

        [TestMethod]
        public void Handle_GivenPOSTRequestWhereNotRoutedReturns404Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "POST",
                Url = "/Test/NotFound"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                }
            });

            httpProcessor.HandleClient(null);

            Assert.AreEqual(HttpStatusCode.NotFound, httpProcessor.Response.HttpStatusCode);
        }

        [TestMethod]
        public void Handle_GivenPOSTRequestWhereNotRoutedButGETIsRouted_Returns405Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "POST",
                Url = "/Test/Example"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                }
            });

            httpProcessor.HandleClient(null);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, httpProcessor.Response.HttpStatusCode);
        }

        [TestMethod]
        public void Handle_GivenPOSTRequest_Returns200Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "POST",
                Url = "/Test/Example",
                ContentAsUTF8 = "Hello World"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "POST",
                Callable = (HttpRequest request) =>
                {

                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                }
            });

            httpProcessor.HandleClient(null);

            Assert.AreEqual(HttpStatusCode.Ok, httpProcessor.Response.HttpStatusCode);
        }
    }
}
