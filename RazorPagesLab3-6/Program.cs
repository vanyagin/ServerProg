using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RazorPagesLab3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public interface IService<T>
    {
        IEnumerable<T> getContent(string filename);
    }

    public class Service<T> : IService<T>
    {
        public IEnumerable<T> getContent(string filename)
        {
            var streamReader = new StreamReader(filename);

            string json = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize<T[]>(json) ?? new T[] { };
        }
    }

    [Serializable]
    public class Product
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
    }

    public class ContactsWriter
    {
        private string filename = "contacts.csv";
        private Mutex mutex = new Mutex(false, "CsvWriterMutex");

        public void WriteInCSV(ContactForm contact)
        {
             var config = new CsvConfiguration(CultureInfo.InvariantCulture)
             {
                 HasHeaderRecord = false,
             };
             mutex.WaitOne();
             using (var writer = new CsvWriter(new StreamWriter(filename, true), config))
             {
                 writer.WriteRecord(contact);
                 writer.NextRecord();
             }
             mutex.ReleaseMutex();   
        }
    }

    public class ContactForm
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? SelectService { get; set; }
        public string? SelectPrice { get; set; }
        public string? Comments { get; set; }
    }

}
