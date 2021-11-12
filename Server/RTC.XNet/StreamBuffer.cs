using System.Collections.Concurrent;
using System.Threading;
using Google.Protobuf;

namespace RTC.XNet
{
    public class StreamBuffer<TData> : System.IDisposable
        where TData : IMessage, new()
    {
        private readonly ConcurrentQueue<TData> _requests = new ConcurrentQueue<TData>();

        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        public CancellationToken CloseToken => _source.Token;
       

        public int Max { get; }

        public StreamBuffer(int max = 100)
        {
            this.Max = max;
        }

        public bool TryPull(out TData data)
        {
            return _requests.TryDequeue(out data);
        }

        public virtual bool Push(TData request)
        {
            if (_requests.Count > Max)
            {
                _requests.TryDequeue(out _);
            }

            _requests.Enqueue(request);
            return true;
        }

        public bool IsWorking { private set; get; } = true;
        
        public virtual void Dispose()
        {
            IsWorking = false;
            _source.Cancel();
        }
    }
}