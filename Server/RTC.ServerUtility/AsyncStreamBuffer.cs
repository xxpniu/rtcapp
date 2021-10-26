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

        public override void Close()
        {
            base.Close();
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
            while (IsWorking)
            {
                await _semaphoreSlim.WaitAsync(cancellationToken: token);
                while (TryPull(out var data))
                {
                    token.ThrowIfCancellationRequested();
                    yield return data;
                }
                token.ThrowIfCancellationRequested();
            }
        }
    }
}