#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RTC.XNet;

namespace RTC.ServerUtility
{
    public abstract class ServerApp<T>:XSingleton<T> where T :class ,new()
    {
        protected abstract Task Start(CancellationToken? token = default);
        protected abstract Task Stop(CancellationToken? token= default);

        private  CancellationTokenSource? _source;

        public async Task RunAsync(CancellationToken? token = default)
        {
            token ??= CancellationToken.None;
            _source = new CancellationTokenSource();
            var link = CancellationTokenSource.CreateLinkedTokenSource(_source.Token, token.Value);
            var host = new HostBuilder();
            
            Debugger.Log($"app starting");
            await Start(link.Token);
            
            try
            {
                //await host.WaitForShutdownAsync(token: token.Value);
                Debugger.Log($"app waiting to shutdown!");
                await host.RunConsoleAsync(link.Token);
                Debugger.Log("Shutting down gracefully.");
            }
            catch (Exception e)
            {
                Debugger.LogError(e.ToString());
            }


            // ReSharper disable once MethodSupportsCancellation
            await Stop();
            Debugger.Log("Exited");
        }
    }
}