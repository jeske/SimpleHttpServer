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
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.HandleClient(null);
            Assert.AreEqual("200", httpProcessor.Response.StatusCode);
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
            Assert.AreEqual("500", httpProcessor.Response.StatusCode);
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
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.HandleClient(null);
            Assert.AreEqual("404", httpProcessor.Response.StatusCode);
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
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.HandleClient(null);


            Assert.AreEqual("200", httpProcessor.Response.StatusCode);
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
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.HandleClient(null);

            Assert.AreEqual("404", httpProcessor.Response.StatusCode);
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
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.HandleClient(null);

            Assert.AreEqual("405", httpProcessor.Response.StatusCode);
        }

        [TestMethod]
        public void Handle_GivenPOSTRequest_Returns200Response()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "POST",
                Url = "/Test/Example",
                Content = "Hello World"
            }, new Route()
            {
                UrlRegex = "^/Test/Example$",
                Method = "POST",
                Callable = (HttpRequest request) =>
                {
                    Assert.AreEqual("Hello World", request.Content);

                    return new HttpResponse()
                    {
                        ContentAsUTF8 = "Hello World",
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.HandleClient(null);

            Assert.AreEqual("200", httpProcessor.Response.StatusCode);
        }
    }
}
