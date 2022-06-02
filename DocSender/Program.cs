using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace DocSender
{
    class PrintProgress : IProgress<string>
    {
        public void Report(string value)
        {
            Console.WriteLine(value);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
            var config = builder.Build();
            int duration = int.Parse(config["Timing:Duration"]);
            DocumentsQueue docQue = new DocumentsQueue(
                new ExternalSystemConnector(), 
                new PrintProgress(), 
                duration);
            docQue.StartQueue();
            // docQue.StopSending();
        }
    }
}