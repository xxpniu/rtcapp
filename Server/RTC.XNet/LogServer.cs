using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace RTC.XNet
{
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false)]
    public class AuthAttribute:Attribute
    {
        public AuthAttribute()
        { }
    }

    public class AuthException : Exception
    {
        public AuthException(string message) : base(message)
        {
        }
    }
    
     public class ServerLoggerInterceptor : Interceptor
    {
        public ServerLoggerInterceptor(LogServer server)
        {
            this.Server = server;
        }

        public delegate bool AuthCheck(ServerCallContext context);

        public void SetAuthCheck(AuthCheck check)
        {
            _authCheck = check;
        }

        private AuthCheck _authCheck;

        public LogServer Server { get; }

        private bool HaveAuth(System.Reflection.MethodInfo info, ServerCallContext context)
        {
            if (info.GetCustomAttributes(typeof(AuthAttribute), false) is AuthAttribute[] auths && auths.Length > 0) return true;
            var auth = _authCheck?.Invoke(context)?? CheckAuthDefault(context);
            return auth;
        }

        private void CheckAppVersion(ServerCallContext context)
        {
            if (!context.GetHeader("app-version", out var ver) 
                || Version.Compare((Version)ver, Server.Version)!= VersionCompare.Equals)
            {
                throw new AuthException($"Version is {ver}");
            }
        }

        private bool CheckAuthDefault(ServerCallContext context)
        {
            
            if (!context.GetHeader("call-key", out var key1)) return false;
            return context.GetHeader("call-token", out var token) && Md5Tool.CheckToken(key1, token);
        }



        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                //LogCall<TRequest, TResponse>(MethodType.Unary, context, request);
                CheckAppVersion(context);
                if (!HaveAuth(continuation.Method, context))
                {
                    LogCall<TRequest, TResponse>(MethodType.Unary, context, request);
                    throw new AuthException("");
                }

                var result = await continuation(request, context);
                var cost = DateTime.UtcNow - startTime;
                LogCall(MethodType.Unary, context, request, result, cost: cost);
                //Debugger.Log($"{typeof(TRequest)} - {typeof(TResponse)} cost time :{cost}");
                return result;
            }
            catch (AuthException)
            {
                //ignore
                throw;
            }
            catch (ResultException)
            {
                throw;
            }
            catch(Exception ex)
            {
                Debugger.LogError($"Error thrown by {context.Method}.{ex}");
                throw;
            }
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context,
            ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.ClientStreaming, context);
            CheckAppVersion(context);
            if (!HaveAuth(continuation.Method, context)) throw new AuthException("auth error");
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.ServerStreaming, context,request);
            CheckAppVersion(context);
            if (!HaveAuth(continuation.Method, context)) throw new AuthException("auth error");
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
            IAsyncStreamReader<TRequest> requestStream,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogCall<TRequest, TResponse>(MethodType.DuplexStreaming, context);
            CheckAppVersion(context);
            if (!HaveAuth(continuation.Method, context)) { throw new AuthException("auth error"); }
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        private static void LogCall<TRequest, TResponse>(MethodType methodType, ServerCallContext context,
            TRequest request = null, TResponse response = null, TimeSpan? cost = default)
            where TRequest : class
            where TResponse : class
        {

            string GetHeader(string key)
            {
                return context.RequestHeaders.Get(key)?.Value;
            }
            
            Debugger.LogIfLevel(LoggerType.Debug,()=>
            {
                var headers = new[]
                    { "caller-user", "caller-machine", "caller-os", "call-key", "call-token", "session-key" };
                var sb = new StringBuilder();
                foreach (var i in headers)
                {
                    var str = GetHeader(i);
                    if (string.IsNullOrEmpty(str)) continue;
                    sb.Append($" {i}={str}");
                }
                return $"Head:{sb}";
            });
            
            
            Debugger.LogIfLevel(LoggerType.Log,
                ()=> cost.HasValue ? $"Cost:{cost?.TotalMilliseconds}ms Call:{methodType} [{typeof(TRequest)}]->{request} Response: {typeof(TResponse)}{response}" 
                    : $"Call:{methodType} [{typeof(TRequest)}]->{request} Response: {typeof(TResponse)}{response}");
        }
    }

    public class LogServer : Server
    {
        public Version Version { get; set; } = new Version("1.0.0");
        public ServerLoggerInterceptor Interceptor { get; } 

        public LogServer() : base(null)
        {
            Interceptor = new ServerLoggerInterceptor(this);
        }

        public LogServer BindServices(params ServerServiceDefinition[] definitions)
        {
            foreach (var i in definitions)
            {
                this.Services.Add(i.Intercept(Interceptor));
            }

            return this;
        }

        private readonly ConcurrentDictionary<string, string> _sessionKey = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, string> _userSession = new ConcurrentDictionary<string, string>();

        public bool TryCreateSession(string accountId,out string sessionKey)
        {
            var gui = Guid.NewGuid().ToString();
            if (_userSession.TryGetValue(accountId, out var oldSession))
            {
                _sessionKey.TryRemove(oldSession, out _);
                _userSession.TryRemove(accountId, out _);
            }
            sessionKey = gui;
            return _sessionKey.TryAdd(gui, accountId) && _userSession.TryAdd(accountId,gui);
        }
        public bool CheckSession(string key,out string value)
        {
            return _sessionKey.TryGetValue(key,out value) ;
        }
    }
}