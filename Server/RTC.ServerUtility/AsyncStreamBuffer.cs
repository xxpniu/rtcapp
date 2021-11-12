using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Google.Protobuf;
using RTC.XNet;

namespace RTC.ServerUtility
{
    public class AsyncStreamBuffer<TData>:StreamBuffer<TData>
        where TData: IMessage,new()
    {
        public AsyncStreamBuffer(int max = 300):base(max)
        {

        }

        public override bool Push(TData request)
        {
            var t= base.Push(request);
            Resume();
            return t;
        }

        public override void Dispose()
        {
            base.Dispose();
            _semaphoreSlim.Dispose();
            Resume();
        }

        private readonly SemaphoreSlim _semaphoreSlim = new(0, 1);

        private void Resume()
        {
            try
            {
                _semaphoreSlim.Release();
            }
            catch
            {
                //ignore
            }
        }

        
        public async IAsyncEnumerable<TData> TryPullAsync([EnumeratorCancellation] CancellationToken token)
        {

            var linked = CancellationTokenSource.CreateLinkedTokenSource(token, this.CloseToken);
            var breakToken = linked.Token;
            while (IsWorking)
            {
                await _semaphoreSlim.WaitAsync(breakToken);
                
                while (TryPull(out var data))
                {
                    breakToken.ThrowIfCancellationRequested();
                    yield return data;
                }

                breakToken.ThrowIfCancellationRequested();
            }
        }
    }
}