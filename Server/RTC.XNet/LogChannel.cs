﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace RTC.XNet
{
    public class ClientLoggerInterceptor : Interceptor
    {
        public  Version Version { set; get; } = new Version("1.0.0");
        public LogChannel Channel { get; }

        public ClientLoggerInterceptor(LogChannel channel)
        {
            this.Channel = channel;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method, request);
            AddCallerMetadata(ref context);
            var call = continuation(request, context);
            
            var asyncCall = new AsyncUnaryCall<TResponse>(
                HandleResponse(call.ResponseAsync),
                call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
            return asyncCall;
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> t)
        {
            try
            {
                var response = await t;
                Debugger.Log($"{typeof(TResponse)}:{response}");
                return response;
            }
            catch (Exception ex)
            {
                Debugger.LogWaring($"Call error: {ex.Message}");
                throw;
            }
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(context);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(request, context);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);
            AddCallerMetadata(ref context);

            return continuation(context);
        }

        private void LogCall<TRequest, TResponse>(Method<TRequest, TResponse> method, TRequest request = null,
            TResponse response = null)
            where TRequest : class
            where TResponse : class
        {
            Debugger.LogIfLevel(LoggerType.Log,
                () =>
                    $"Starting Type: [{method.Type}] Req: {typeof(TRequest)}{request}. Res: {typeof(TResponse)}{response}");

        }

        private void AddCallerMetadata<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var headers = context.Options.Headers;

            // Call doesn't have a headers collection to add to.
            // Need to create a new context with headers for the call.
            if (headers == null)
            {
                headers = new Metadata();
                var options = context.Options.WithHeaders(headers);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
            }

            var time = $"{DateTime.Now.Ticks}{Channel.Token}";
            //headers.Add("caller-user", Environment.UserName);
            //headers.Add("caller-machine", Environment.MachineName);
            headers.Add("caller-os", Environment.OSVersion.ToString());
            headers.Add("call-key", Md5Tool.GetTokenKey(time));
            headers.Add("call-token", time);
            headers.Add("session-key", Channel.SessionKey ?? string.Empty);
            headers.Add("app-version", Version.ToString());
            //headers.Add("request-time",DateTime.UtcNow.Ticks.ToString());
        }
    }

    public class LogChannel : Channel
    {
        public string Token { get; set; }
        public string SessionKey { set; get; }

        public LogChannel(string address, int port) : this($"{address}:{port}",
            ChannelCredentials.Insecure)
        {
        }

        public LogChannel(string target, ChannelCredentials credentials) : base(target, credentials)
        {
            //Debugger.Log($"LogChannel:{target}");
            Debugger.LogIfLevel(LoggerType.Log, () => $"LogChannel:{target}");
        }

        private CallInvoker CreateLogCallInvoker()
        {
            return this.Intercept(new ClientLoggerInterceptor(this));
        }

        public async Task<T> CreateClientAsync<T>(DateTime? deadline = default) where T : ClientBase
        {
            if (deadline == null) deadline = DateTime.UtcNow.AddSeconds(10);
            await this.ConnectAsync(deadline);

            return CreateClient<T>();
        }

        private T CreateClient<T>() where T : ClientBase
        {
            return Activator.CreateInstance(typeof(T), this.CreateLogCallInvoker()) as T;
        }

        public class OneRequest<T> where T : ClientBase
        {
            private readonly string _endPoint;
            public OneRequest(string endPoint)
            {
                _endPoint = endPoint;
            }
            
            public  async Task<TRes> SendRequestAsync<TRes>( Func<T, Task<TRes>> requestInvoke) 
                where TRes : IMessage
            {
                var log = new LogChannel(_endPoint, ChannelCredentials.Insecure);
                var client = await log.CreateClientAsync<T>();
                var res = await requestInvoke.Invoke(client);
                await log.ShutdownAsync();
                return res;
            }
        }

        public static OneRequest<T> CreateOneRequest<T>(string endPoint)
            where T : ClientBase
        {
            return new OneRequest<T>(endPoint);
        }
    }

}