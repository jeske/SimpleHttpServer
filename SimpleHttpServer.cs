using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// offered to the public domain for any use with no restriction
// and also with no warranty of any kind, please enjoy. - David Jeske. 

// simple HTTP explanation
// http://www.jmarshall.com/easy/http/

namespace Minimalistic {

    public class HttpProcessor {
        public TcpClient Socket;        
        public HttpServer Srv;

        private Stream inputStream;
        public StreamWriter OutputStream;

        public String HttpMethod;
        public String HttpUrl;
        public String HttpProtocolVersionstring;
        public Hashtable HttpHeaders = new Hashtable();


	    private const int MaxPostSize = 10*1024*1024; // 10MB

	    public HttpProcessor(TcpClient s, HttpServer srv) {
            Socket = s;
            Srv = srv;                   
        }
        

        private static string StreamReadLine(Stream inputStream) {
	        var data = "";
            while (true) {
                var nextChar = inputStream.ReadByte();
                if (nextChar == '\n') { break; }
	            switch (nextChar)
	            {
		            case '\r':
			            continue;
		            case -1:
			            Thread.Sleep(1);
			            continue;
	            }
	            data += Convert.ToChar(nextChar);
            }            
            return data;
        }
        public void Process() {                        
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            inputStream = new BufferedStream(Socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            OutputStream = new StreamWriter(new BufferedStream(Socket.GetStream()));
            try {
                ParseRequest();
                ReadHeaders();
                if (HttpMethod.Equals("GET")) {
                    HandleGetRequest();
                } else if (HttpMethod.Equals("POST")) {
                    HandlePostRequest();
                }
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.ToString());
                WriteFailure();
            }
            OutputStream.Flush();
            // bs.Flush(); // flush any remaining output
            inputStream = null; OutputStream = null; // bs = null;            
            Socket.Close();             
        }

        public void ParseRequest() {
            var request = StreamReadLine(inputStream);
            var tokens = request.Split(' ');
            if (tokens.Length != 3) {
                throw new Exception("invalid http request line");
            }
            HttpMethod = tokens[0].ToUpper();
            HttpUrl = tokens[1];
            HttpProtocolVersionstring = tokens[2];

            Console.WriteLine("starting: " + request);
        }

        public void ReadHeaders() {
            Console.WriteLine("readHeaders()");
            String line;
            while ((line = StreamReadLine(inputStream)) != null) {
                if (line.Equals("")) {
                    Console.WriteLine("got headers");
                    return;
                }
                
                var separator = line.IndexOf(':');
                if (separator == -1) {
                    throw new Exception("invalid http header line: " + line);
                }
                var name = line.Substring(0, separator);
                var pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' ')) {
                    pos++; // strip any spaces
                }
                    
                var value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}",name,value);
                HttpHeaders[name] = value;
            }
        }

        public void HandleGetRequest() {
            Srv.HandleGetRequest(this);
        }

        private const int BufSize = 4096;
        public void HandlePostRequest() {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            Console.WriteLine("get post data start");
	        var ms = new MemoryStream();
            if (HttpHeaders.ContainsKey("Content-Length")) {
                 var contentLen = Convert.ToInt32(HttpHeaders["Content-Length"]);
                 if (contentLen > MaxPostSize) {
                     throw new Exception(
                         String.Format("POST Content-Length({0}) too big for this simple server",
                           contentLen));
                 }
                 var buf = new byte[BufSize];              
                 var toRead = contentLen;
                 while (toRead > 0) {  
                     Console.WriteLine("starting Read, to_read={0}",toRead);

                     var numread = inputStream.Read(buf, 0, Math.Min(BufSize, toRead));
                     Console.WriteLine("read finished, numread={0}", numread);
                     if (numread == 0)
                     {
	                     if (toRead == 0) {
                             break;
                         }
	                     throw new Exception("client disconnected during post");
                     }
	                 toRead -= numread;
                     ms.Write(buf, 0, numread);
                 }
                 ms.Seek(0, SeekOrigin.Begin);
            }
            Console.WriteLine("get post data end");
            Srv.HandlePostRequest(this, new StreamReader(ms));

        }

        public void WriteSuccess(string contentType="text/html") {
            // this is the successful HTTP response line
            OutputStream.WriteLine("HTTP/1.0 200 OK");  
            // these are the HTTP headers...          
            OutputStream.WriteLine("Content-Type: " + contentType);
            OutputStream.WriteLine("Connection: close");
            // ..add your own headers here if you like

            OutputStream.WriteLine(""); // this terminates the HTTP headers.. everything after this is HTTP body..
        }

        public void WriteFailure() {
            // this is an http 404 failure response
            OutputStream.WriteLine("HTTP/1.0 404 File not found");
            // these are the HTTP headers
            OutputStream.WriteLine("Connection: close");
            // ..add your own headers here

            OutputStream.WriteLine(""); // this terminates the HTTP headers.
        }
    }

    public abstract class HttpServer {

        protected int Port;
        TcpListener listener;

	    protected HttpServer(int port) {
            Port = port;
        }

        public void Listen() {
            listener = new TcpListener(Dns.GetHostAddresses("localhost").First(),Port);
            listener.Start();
            while (true) {                
                var s = listener.AcceptTcpClient();
                var processor = new HttpProcessor(s, this);
                var thread = new Thread(processor.Process);
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void HandleGetRequest(HttpProcessor p);
        public abstract void HandlePostRequest(HttpProcessor p, StreamReader inputData);
    }

    public class MyHttpServer : HttpServer {
        public MyHttpServer(int port)
            : base(port) {
        }
        public override void HandleGetRequest (HttpProcessor p)
		{

			if (p.HttpUrl.Equals ("/Fiddle.html")) {
				Stream fs = File.Open("Fiddle.html",FileMode.Open);

				p.WriteSuccess();
				//fs.CopyTo (p.OutputStream.BaseStream);
				//p.OutputStream.BaseStream.Flush();
				var text = (new StreamReader(fs, Encoding.UTF8)).ReadToEnd();
				p.OutputStream.Write(text);
				return;
			}

            Console.WriteLine("request: {0}", p.HttpUrl);
            p.WriteSuccess();
            p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
            p.OutputStream.WriteLine("Current Time: " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
            p.OutputStream.WriteLine("url : {0}", p.HttpUrl);

            p.OutputStream.WriteLine("<form method=post action=/form>");
            p.OutputStream.WriteLine("<input type=text name=foo value=foovalue>");
            p.OutputStream.WriteLine("<input type=submit name=bar value=barvalue>");
            p.OutputStream.WriteLine("</form>");
        }

        public override void HandlePostRequest(HttpProcessor p, StreamReader inputData) {
            Console.WriteLine("POST request: {0}", p.HttpUrl);
            var data = inputData.ReadToEnd();

            p.WriteSuccess();
            p.OutputStream.WriteLine("<html><body><h1>test server</h1>");
            p.OutputStream.WriteLine("<a href=/test>return</a><p>");
            p.OutputStream.WriteLine("postbody: <pre>{0}</pre>", data);
            

        }
    }

    public class TestMain {
        public static int Main(String[] args) {
            var httpServer = args.GetLength(0) > 0 ? new MyHttpServer(Convert.ToInt16(args[0])) : new MyHttpServer(8080);
            var thread = new Thread(httpServer.Listen);
            thread.Start();
            return 0;
        }

    }

}



