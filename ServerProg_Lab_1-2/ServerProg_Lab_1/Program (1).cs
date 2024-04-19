using System;
using System.IO;
using System.Net;

namespace ServerProg_Lab_1
{
    class Program
    {

        private static void CreateLog(HttpListenerContext context, HttpListenerResponse response)
        {
            using (StreamWriter sw = File.AppendText("log.txt"))
            {
                sw.WriteLine($"Дата обращения: {DateTime.Now}");
                sw.WriteLine($"Адрес клиента: {context.Request.RemoteEndPoint}");
                sw.WriteLine($"Адрес приложения: {context.Request.LocalEndPoint}");
                sw.WriteLine($"Запрошен путь: {context.Request.Url}");
                sw.WriteLine($"Код ответа: {response.StatusCode}");
                sw.WriteLine("________________________________");
            }
        }

        static void Main(string[] args)
        {
            string[] prefixes = { "" };


            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8888/");
            listener.Start();
            Console.WriteLine("Listening...");
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                Console.WriteLine(context.Request.Url);
                Console.WriteLine(context.Request.RemoteEndPoint);

                string filepath = context.Request.Url.LocalPath.TrimStart('/');
                Console.WriteLine(filepath);

                if (!File.Exists(filepath))
                {
                    HttpListenerResponse response = context.Response;
                    response.StatusCode = 404;
                    CreateLog(context, response);
                    response.OutputStream.Close();
                }
                else
                {
                    HttpListenerResponse response = context.Response;
                    System.IO.Stream input = File.OpenRead(filepath);
                    byte[] buf = new byte[1024];
                    int bytesRead = input.Read(buf, 0, buf.Length);
                    while(bytesRead > 0)
                    {
                        response.OutputStream.Write(buf, 0, bytesRead);
                        bytesRead = input.Read(buf, 0, buf.Length);

                    }
                    response.StatusCode = 200;
                    CreateLog(context, response);
                    input.Close();
                    response.OutputStream.Close();
                }


                

            }

            listener.Stop();
        }
    }
}
