using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.SessionServer;
using RTC.ProtoGrpc.SignalServer;
using RTC.ServerUtility;
using RTC.XNet;

namespace SignalServer.GrpcHandler
{
    public class SignalService : SignalServerService.SignalServerServiceBase
    {
        public SignalService(WatcherServer<string, ServerDefine> watcher)
        {
            _watcher = watcher;
        }

        private readonly WatcherServer<string, ServerDefine> _watcher;
        
        private readonly ConcurrentDictionary<string, AsyncStreamBuffer<RouteMessage>> _connections =
            new();

        public override async Task<Void> Route(RouteMessage request, ServerCallContext context)
        {
            if (_connections.TryGetValue(request.ToId, out var t))
            {
                t.Push(request);
            }
            return await Task.FromResult(new Void());
        }

        public override async Task Connect(C2S_Connect request, IServerStreamWriter<RouteMessage> responseStream, ServerCallContext context)
        {
            var accountId = request.Session.AccountId;
            var sso = _watcher.AnyOne();
            
            var channel = new LogChannel(sso.Address, sso.Port);
            
            var client = await channel.CreateClientAsync<SessionService.SessionServiceClient>();
            var ssoCheck = await client.CheckSessionAsync(new O2S_CheckSession
            {
                Token = request.Session.Token,
                AccountId = request.Session.AccountId
            }, cancellationToken: context.CancellationToken);

            await channel.ShutdownAsync();

            if (!ssoCheck.Available)
            {
                throw new AuthException("session checked error!");
            }


            if (_connections.TryGetValue(accountId, out var t))
            {
                t.Close();
                if(!_connections.TryRemove(accountId, out _)) return;
            }

            var oneLine = new PeerOnline
            {
                SessionId =accountId
            };
            
            var buffer = new AsyncStreamBuffer<RouteMessage>();
            foreach (var player in _connections)
            {
                var rout = new RouteMessage
                {
                    FromId = accountId,
                    ToId = player.Key,
                    Msg = Any.Pack(oneLine)
                };
                player.Value.Push(rout);
            }

            _connections.TryAdd(accountId, buffer);
            
            try
            {
                await foreach (var message in buffer.TryPullAsync(context.CancellationToken))
                {
                    await responseStream.WriteAsync(message);
                }
            }
            catch
            {
                //ignore
            }

            _connections.TryRemove(accountId, out _);
            var offLine = new PeerOffLine()
            {
                SessionId = accountId
            };

            foreach (var player in _connections)
            {
                var rout = new RouteMessage
                {
                    FromId = accountId,
                    ToId = player.Key,
                    Msg = Any.Pack(offLine)
                };
                player.Value.Push(rout);
            }
        }

        public override async Task<S2C_QueryPlayerList> QueryPlayerList(C2S_QueryPlayerList request,
            ServerCallContext context)
        {
            var response = new S2C_QueryPlayerList
            {
                Playies = {_connections.Keys}
            };

            return await Task.FromResult(response);
        }
    }
}