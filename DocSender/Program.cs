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

            using (DocumentsQueue docQueue = new DocumentsQueue(
                       new ExternalSystemConnector(),
                       new PrintProgress(),
                       duration))
            {
                // Запускаем сервис формирования пакетов по 10 документов
                // и отправки их в ExternalSystemConnector.
                // Он будет крутиться в отдельном потоке (точнее всякий раз в новых)
                // Закончится, когда выйдем из using. 
                
                docQueue.QueueSenderAsync();

                docQueue.Enqueue(new Document() { Id = 1, Title = "First doc", DocumentType = "AAA" });
                Thread.Sleep(1000);
                int i = 2;
                for (; i < 20; i++)
                {
                    docQueue.Enqueue(new Document() { Id = i, Title = $"{i}th doc", DocumentType = "BBB" });
                }
                Thread.Sleep(3000);
                for (; i < 34; i++)
                {
                    docQueue.Enqueue(new Document() { Id = i, Title = $"{i}th doc", DocumentType = "BBB" });
                }
                // Это надо, потому что предыдущая команда запускает процесс в бэкграунде
                // и сразу завершается
                Console.ReadKey();
            }
        }
    }
}