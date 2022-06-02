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
        
        public async Task StartQueue()
        {
            _progress.Report($"Is cancellation requested {_token.IsCancellationRequested}");
            Task t = new Task(() => SenderAsync(_token));
            t.Start();
            await t;
        }

        // Options
        // 1. Пришёл документ - сразу отправили 
        public void Enqueue(Document document)
        {
            new Task(() => _mainQueue.Enqueue(document)).Start();
        }

        private async Task SenderAsync(CancellationToken token)
        {
            _progress.Report("Sender Async started");
            Document nextDoc;
            do
            {
                _progress.Report("Sender Async started do statement");
                _progress.Report($"Is cancellation requested {token.IsCancellationRequested}");
                _progress.Report($"main queue is empty: {_mainQueue.IsEmpty}");
                while (!_mainQueue.IsEmpty)
                {
                    List<Document> portionToSend = new(10);
                    for (int i = 0;!_mainQueue.IsEmpty && i < 10; i++)
                    {
                        _mainQueue.TryDequeue(out nextDoc);
                        portionToSend.Add(nextDoc);
                    }

                    try
                    {
                        await _externalSystemConnector.SendDocuments(new ReadOnlyCollection<Document>(portionToSend), token);
                    }
                    catch (Exception e)
                    {
                        await DisposeAsync().ConfigureAwait(false);
                        _progress.Report("Document sender was stopped. Document queue is empty.");
                        return;
                    }
                }
                
                try
                {
                    _progress.Report("SEnder Async entered try Task.Delay");
                    await Task.Delay(_duration, token); 
                    _progress.Report("SEnder Async after Task.Delay");
                }
                catch (Exception e)
                {
                    await DisposeAsync().ConfigureAwait(false);
                    _progress.Report("Document sender was stopped. Document queue is empty.");
                    return;
                }
                
            } while (true);
        }

        public void StopSending()
        {
            _cTS.Cancel();
            _cTS.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            if (!_mainQueue.IsEmpty)
            {
                _mainQueue.Clear();
            }
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            if (!_mainQueue.IsEmpty)
            {
                _mainQueue.Clear();
            }
        }

    }
}