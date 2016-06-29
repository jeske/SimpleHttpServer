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
        public void Handle_GivenGETRequest_ReturnsOkResponse()
        {
            MockHttpProcessor httpProcessor = new MockHttpProcessor(new HttpRequest()
            {
                Method = "GET",
                Url = "/Test/Example"
            }, new Route()
            {
                Url = "/Test/Example",
                Method = "GET",
                Callable = (HttpRequest request) =>
                {
                    return new HttpResponse()
                    {
                        Content = "Hello World",
                        ReasonPhrase = "OK",
                        StatusCode = "200"
                    };
                }
            });

            httpProcessor.Handle(null);


            Assert.AreEqual("200", httpProcessor.Response.StatusCode);
        }
    }
}
