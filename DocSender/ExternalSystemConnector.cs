using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DocSender
{
    /// <summary>
    /// Представляет коннектор для отправки документов во внешнюю систему.
    /// </summary>
    public sealed class ExternalSystemConnector
    {
        /// <summary>
        /// Выполняет отправку документов во внешнюю систему.
        /// </summary>
        /// <param name="documents">
        /// Документы, которые нужно отправить.
        /// </param>
        /// <param name="cancellationToken">
        /// <see cref="CancellationToken"/> для отмены асинхронной операции.
        /// </param>
        /// <returns>
        /// Асинхронная операция, завершение которой означает успешную отправку документов.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Возникакет при попытке отправить более 10 документов за раз.
        /// </exception>
        public async Task SendDocuments(IReadOnlyCollection<Document> documents, CancellationToken cancellationToken) 
        {
            if (documents.Count > 10) 
            {
                throw new ArgumentException("Can't send more than 10 documents at once.", nameof(documents));
            }

            foreach (var d in documents)
            {
                Console.WriteLine($"{d.Id} {d.Title} was sent");
                
            }
            // тестовая реализация, просто ничего не делаем 2 секунды
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); 
        }
    }
}