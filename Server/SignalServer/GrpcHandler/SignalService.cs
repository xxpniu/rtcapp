using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.SessionServer;
using RTC.ProtoGrpc.SignalServer;
using RTC.ServerUtility;
using RTC.XNet;
using SignalServer.Utility;
using Void = RTC.ProtoGrpc.Data.Void;

namespace SignalServer.GrpcHandler
{
    public class SignalService : SignalServerService.SignalServerServiceBase
    {
        public SignalService(WatcherServer<string, ServerDefine> watcher)
        {
            _watcher = watcher;
        }

        private readonly WatcherServer<string, ServerDefine> _watcher;
        
        private readonly ConcurrentDictionary<string, RoomClient> _connections =
            new();

        public override async Task<Void> Route(RouteMessage request, ServerCallContext context)
        {
            if (!RouteMessageToOther(request))
            {
                Debugger.LogWaring($"{request} send error");
            }

            return await Task.FromResult(new Void());
        }
        
        private bool  RouteMessageToOther(RouteMessage message)
        {
            
            if (!_connections.TryGetValue(message.ToId, out var to) ||
                !_connections.TryGetValue(message.FromId, out var from))
                return false;
            
            if (string.Equals(to.RoomId, @from.RoomId))
            {
                to.Push(message);
                return true;
            }

            Debugger.LogWaring($"from {@from.RoomId} to {to.RoomId} not equals");

            return false;
        }

        public override async Task Connect(C2S_Connect request, IServerStreamWriter<RouteMessage> responseStream,
            ServerCallContext context)
        {
            try
            {
                var accountId = request.Session.AccountId;

                var sso = _watcher.AnyOne();

                var re = new O2S_CheckSession
                {
                    Token = request.Session.Token,
                    AccountId = request.Session.AccountId
                };

                var ssoCheck = await LogChannel
                    .CreateOneRequest<SessionService.SessionServiceClient>(sso.ToEndPoint())
                    .SendRequestAsync(async c => await c.CheckSessionAsync(re));

                if (!ssoCheck.Available) throw new AuthException("session checked error!");
                

                Debugger.LogIfLevel(LoggerType.Log,()=>$"{accountId} joined");

                if (_connections.TryGetValue(accountId, out var t))
                {
                    t.Dispose();
                    if (!_connections.TryRemove(accountId, out _)) return;
                }
                
                

                var online = new PeerOnline
                {
                    SessionId = accountId
                };

                var buffer = new RoomClient(request.RoomId, new AsyncStreamBuffer<RouteMessage>());

                foreach (var (key, value) in _connections)
                {
                    RouteMessageToOther(online.ToRoute(accountId, key));
                }

                _connections.TryAdd(accountId, buffer);

                try
                {
                    await foreach (var message in buffer.Buffer.TryPullAsync(context.CancellationToken))
                    {
                        await responseStream.WriteAsync(message);
                    }
                }
                catch
                {

                    //ignore
                }



                buffer.Dispose();


                //remove if close 
                if (_connections.TryGetValue(accountId, out var b))
                {
                    if (!b.Buffer.IsWorking)
                    {
                        _connections.TryRemove(accountId, out _);

                        var offLine = new PeerOffLine()
                        {
                            SessionId = accountId
                        };

                        foreach (var (key, _) in _connections)
                        {
                            RouteMessageToOther(offLine.ToRoute(accountId, key));
                        }
                    }
                }
                
                Debugger.LogIfLevel(LoggerType.Log,()=>$"{accountId} exited");
            }
            catch(Exception exception)
            {
                Debugger.LogError(exception);
            }
        }

        public override async Task<S2C_QueryPlayerList> QueryPlayerList(C2S_QueryPlayerList request,
            ServerCallContext context)
        {

            var clients = _connections
                .Where(t => t.Value.RoomId == request.RoomId 
                            && t.Value.Buffer.IsWorking)
                .Select(t => t.Key)
                .ToList();
            
            var response = new S2C_QueryPlayerList
            {
                Playies = {  clients }
            };

            return await Task.FromResult(response);
        }
    }
}