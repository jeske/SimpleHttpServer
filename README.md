# Simple HTTP Server in C#

**David Jeske** — December 20, 2010

*Threaded synchronous HTTP Server abstract class, to respond to HTTP requests.*

*Originally published on CodeProject.com (now defunct). Preserved from archive.org.*
*Source code: [github.com/jeske/SimpleHttpServer](https://github.com/jeske/SimpleHttpServer)*

---

## Introduction

This article covers a simple HTTP server class which you may incorporate into your own projects, or review to learn more about the HTTP protocol.

## Background

High performance web services are often hosted in rock solid webservices like IIS, Apache, or Tomcat. However, HTML is such a flexible UI language, that it can be useful to serve an HTML UI out of practically any application or backend server. In these situations, the overhead and configuration complexity of an external webserver is seldom worth the trouble. What's needed is a simple HTTP class which can be easily embedded to service simple web requests. This class meets that need.

## Using the Code

First let's review how to use the class, and then we'll dig into some of the details of how it operates. We begin by subclassing `HttpServer` and providing implementations for the two abstract methods `handleGETRequest` and `handlePOSTRequest`:

```csharp
public class MyHttpServer : HttpServer {
    public MyHttpServer(int port)
        : base(port) {
    }

    public override void handleGETRequest(HttpProcessor p) {
        Console.WriteLine("request: {0}", p.http_url);
        p.writeSuccess();
        p.outputStream.WriteLine("<html><body><h1>test server</h1>");
        p.outputStream.WriteLine("Current Time: " + DateTime.Now.ToString());
        p.outputStream.WriteLine("url : {0}", p.http_url);

        p.outputStream.WriteLine("<form method=post action=/form>");
        p.outputStream.WriteLine("<input type=text name=foo value=foovalue>");
        p.outputStream.WriteLine("<input type=submit name=bar value=barvalue>");
        p.outputStream.WriteLine("</form>");
    }

    public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData) {
        Console.WriteLine("POST request: {0}", p.http_url);
        string data = inputData.ReadToEnd();

        p.outputStream.WriteLine("<html><body><h1>test server</h1>");
        p.outputStream.WriteLine("<a href=/test>return</a><p>");
        p.outputStream.WriteLine("postbody: <pre>{0}</pre>", data);
    }
}
```

Once a simple request processor is provided, one must instantiate the server on a port, and start a thread for the main server listener.

```csharp
HttpServer httpServer = new MyHttpServer(8080);
Thread thread = new Thread(new ThreadStart(httpServer.listen));
thread.Start();
```

If you compile and run the sample project, you should be able to point a web-browser of choice at `http://localhost:8080` to see the above simple HTML pages rendered. Let's take a brief look at what's going on under the hood.

## Under the Hood

This simple webserver is broken into two components. The `HttpServer` class opens a `TcpListener` on the incoming port, and sits in a loop handling incoming TCP connect requests using `AcceptTcpClient()`. This is the first step of handling an incoming TCP connection. The incoming request arrived on our "well known port", and this accept process creates a fresh port-pair for server to communicate with this client on. That fresh port-pair is our `TcpClient` session. This keeps our main accept port free to accept new connections. As you can see in the code below, each time the listener returns a new `TcpClient`, `HttpServer` creates a new `HttpProcessor` and starts a new thread for it to operate in. This class also contains the abstract methods our subclass must implement in order to produce a response.

```csharp
public abstract class HttpServer {

    protected int port;
    TcpListener listener;
    bool is_active = true;

    public HttpServer(int port) {
        this.port = port;
    }

    public void listen() {
        listener = new TcpListener(port);
        listener.Start();
        while (is_active) {
            TcpClient s = listener.AcceptTcpClient();
            HttpProcessor processor = new HttpProcessor(s, this);
            Thread thread = new Thread(new ThreadStart(processor.process));
            thread.Start();
            Thread.Sleep(1);
        }
    }

    public abstract void handleGETRequest(HttpProcessor p);
    public abstract void handlePOSTRequest(HttpProcessor p, StreamReader inputData);
}
```

At this point, the new client-server TCP connection is handed off to the `HttpProcessor` in its own thread. The `HttpProcessor`'s job is to properly parse the HTTP headers, and hand control to the proper abstract method handler implementation. Let's look at just a few small parts of the HTTP header processing. The first line of an HTTP Request resembles the following:

```
GET /myurl HTTP/1.0
```

After setting up the input and output stream in `process()`, our `HttpProcessor` calls `parseRequest()`, where the above HTTP request line is received and parsed.

```csharp
public void parseRequest() {
    String request = inputStream.ReadLine();
    string[] tokens = request.Split(' ');
    if (tokens.Length != 3) {
        throw new Exception("invalid http request line");
    }
    http_method = tokens[0].ToUpper();
    http_url = tokens[1];
    http_protocol_versionstring = tokens[2];

    Console.WriteLine("starting: " + request);
}
```

The HTTP request line is always three parts, so we simply use a `string.Split()` call to separate it into three pieces. The next step is to receive and parse the HTTP headers from the client. Each header-line includes a type of the form `KEY:Value`. An empty line signifies the end of the HTTP headers. Our code to `readHeaders` is the following:

```csharp
public void readHeaders() {
    Console.WriteLine("readHeaders()");
    String line;
    while ((line = inputStream.ReadLine()) != null) {
        if (line.Equals("")) {
            Console.WriteLine("got headers");
            return;
        }

        int separator = line.IndexOf(':');
        if (separator == -1) {
            throw new Exception("invalid http header line: " + line);
        }
        String name = line.Substring(0, separator);
        int pos = separator + 1;
        while ((pos < line.Length) && (line[pos] == ' ')) {
            pos++; // strip any spaces
        }

        string value = line.Substring(pos, line.Length - pos);
        Console.WriteLine("header: {0}:{1}", name, value);
        httpHeaders[name] = value;
    }
}
```

For each line, we look for the colon separator, grabbing the string before as a name, and the string after as a value. When we reach an empty header-line, we return because we have received all headers.

At this point, we know enough to handle our simple GET or POST, so we dispatch to the proper handler. In the case of a post, there is some trickiness to deal with in accepting the post data. One of the request headers includes the content-length of the post data. While we wish to let our subclass's `handlePOSTRequest` actually deal with the post data, we need to only allow them to request content-length bytes off the stream, otherwise they will be stuck blocking on the input stream waiting for data which will never arrive. In this simple server, we handle this situation with the dirty but effective strategy of reading all the post data into a `MemoryStream` before sending this data to the POST handler. This is not ideal for a number of reasons. First, the post data may be large. In fact it may be a file upload, in which case buffering it into memory may not be efficient or even possible. Ideally, we would create some type of stream-imitator that could be setup to limit itself to content-length bytes, but otherwise act as a normal stream. This would allow the POST handler to pull data directly off the stream without the overhead of buffering in memory. However, this is also much more code. In many embedded HTTP servers, post requests are not necessary at all, so we avoid this situation by simply limiting POST input data to no more than 10MB.

Another simplification of this simple server is the content-type of the return data. In the HTTP protocol, the server always sends the browser the MIME-Type of the data which it should be expecting. In `writeSuccess()`, you can see that this server always indicates a content-type of `text/html`. If you wish to return other content types, you will need to extend this method to allow your handler to supply a content type response before it sends data to the client.

## Points of Interest

This SimpleHttpServer only implements a very bare-bones subset of even the basic HTTP/1.0 spec. Further revisions of the HTTP specification have included more complex and very valuable improvements, including compression, session keep alive, chunked responses, and lots more. However, because of the excellent and simple design of HTTP, you'll find that even this very bare-bones code is capable of serving pages which are compatible with modern web-browsers.

## History

- December 19, 2010: Initial version posted
- December 22, 2010: Removed `StreamReader` from input side so we can properly get raw POST data
- March 2, 2012: Corrected line-terminators to use `WriteLine()` for `\r\n` instead of just `\n`

---

*Originally published December 20, 2010 on CodeProject.com.*
*Copyright David Jeske. Licensed under Apache License, Version 2.0.*
