using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace ServerProg_Lab_1
{
    class Program
    {
        
        static bool KeepGoing = true;
        static List<Task> OngoingTasks = new List<Task>();

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

        private static void ConsoleLog(HttpListenerContext context, HttpListenerResponse response)
        {
            Console.Out.WriteLineAsync($"Дата обращения: {DateTime.Now}");
            Console.Out.WriteLineAsync($"Адрес клиента: {context.Request.RemoteEndPoint}");
            Console.Out.WriteLineAsync($"Адрес приложения: {context.Request.LocalEndPoint}");
            Console.Out.WriteLineAsync($"Запрошен путь: {context.Request.Url}");
            Console.Out.WriteLineAsync($"Код ответа: {response.StatusCode}");
            Console.Out.WriteLineAsync("________________________________");
        }


        static async Task ProcessAsync(HttpListener l)
        {
            while (KeepGoing)
            {
                if (OngoingTasks.Count < 10)
                {
                    var task = l.GetContextAsync().ContinueWith((t) => { 
                        if (t.IsCompletedSuccessfully) Perform(t.Result); 
                    });
                    OngoingTasks.Add(task);
                }
                else
                {
                    await Task.WhenAny(OngoingTasks);
                }

                OngoingTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        private static async Task Perform(HttpListenerContext context)
        {
            string filepath = context.Request.Url.LocalPath.TrimStart('/');

            if (!File.Exists(filepath))
            {
                HttpListenerResponse response = context.Response;
                response.StatusCode = 404;
                CreateLog(context, response);
                ConsoleLog(context, response);
                response.OutputStream.Close();
            }
            else if (context.Request.Url.ToString() == "http://127.0.0.1:8888/CSS Zen Garden_ The Beauty of CSS Design_files/img-bottle.png")
            {
                Task.Delay(3000).Wait();
                HttpListenerResponse response = context.Response;
                byte[] buffer = await File.ReadAllBytesAsync(filepath);
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 202;
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
                CreateLog(context, response);
                ConsoleLog(context, response);
                response.OutputStream.Close();
                output.Close();
            }
            else
            {
                HttpListenerResponse response = context.Response;
                byte[] buffer = await File.ReadAllBytesAsync(filepath);
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
                CreateLog(context, response);
                ConsoleLog(context, response);
                response.OutputStream.Close();
                output.Close();
            }
        }

        static async Task Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8888/");
            listener.Start();
            Console.WriteLine("Listening...");

            while (listener.IsListening)
            {
                await ProcessAsync(listener);
            }
        }
    }
}
