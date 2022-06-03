using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace DocSender
{
    public class DocumentsQueue : IDocumentsQueue, IAsyncDisposable, IDisposable
    {
        private readonly ExternalSystemConnector _externalSystemConnector;
        private readonly IProgress<string> _progress;
        private readonly ConcurrentQueue<Document> _mainQueue;
        private readonly CancellationTokenSource _cTS;
        private readonly CancellationToken _token;
        private readonly Task _senderAsync;
        private readonly TimeSpan _duration;

        public DocumentsQueue(ExternalSystemConnector extSC, IProgress<string> progress, int durationSec)
        {
            _externalSystemConnector = extSC;
            _progress = progress;
            _mainQueue = new();
            _duration = TimeSpan.FromSeconds(durationSec);
            
            _cTS = new CancellationTokenSource();
            _token = _cTS.Token;
        }
        
        // public async Task StartQueue()
        // {
        //     await Task.Factory.StartNew(QueueSenderAsync,
        //         _token,
        //         TaskCreationOptions.LongRunning,
        //         TaskScheduler.Default);
        // }

        public void Enqueue(Document document)
        {
            // Сделал именно Task.Run на случай,
            // если через API будет поступать слишком много запросов
            // Task.Run(() => _mainQueue.Enqueue(document));
            _mainQueue.Enqueue(document);
        }

        public async Task QueueSenderAsync()
        {
            _progress.Report("Document queue sender started");
            do
            {
                while (!_mainQueue.IsEmpty)
                {
                    List<Document> portionToSend = new(10);
                    int i;
                    for (i = 0;!_mainQueue.IsEmpty && i < 10; i++)
                    {
                        _mainQueue.TryDequeue(out Document nextDoc);
                        portionToSend.Add(nextDoc);
                    }
                    try
                    {
                        await _externalSystemConnector.SendDocuments(new ReadOnlyCollection<Document>(portionToSend), _token);
                        _progress.Report($"{i} documents were sent.");
                    }
                    catch (Exception e)
                    {
                        Dispose();
                        _progress.Report($"{e.Message}");
                        _progress.Report("Document sending was unexpectedly stopped. Document queue is empty.");
                        return;
                    }
                }
                
                try
                {
                    await Task.Delay(_duration, _token); 
                }
                catch (Exception e)
                {
                    _progress.Report("Document sender was stopped. Document queue is empty.");
                    return;
                }
                
            } while (true);
        }

        private void StopSending()
        {
            _cTS.Cancel();
            _cTS.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            _progress.Report("DisposeAsync was called");
            if (!_mainQueue.IsEmpty)
            {
                _mainQueue.Clear();
            }
            StopSending();
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            _progress.Report("Dispose was called");
            if (!_mainQueue.IsEmpty)
            {
                _mainQueue.Clear();
            }
            StopSending();
        }

    }
}