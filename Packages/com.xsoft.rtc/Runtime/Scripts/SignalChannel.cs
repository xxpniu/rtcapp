using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Utils;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.SignalServer;
using RTC.XNet;
using Unity.WebRTC;
using UnityEngine;

namespace XRTC
{
   public class SignalChannel: IDisposable
    {
        public struct EventData<T>
        {
            public string FromID;
            public T Data;
        }
        
        private readonly string _ip;
        private readonly int _port;
        private readonly string _sessionId;
        private readonly TokenSession _tokenSession;
        
        private SignalServerService.SignalServerServiceClient _service;
        private LogChannel _logChannel;
        private AsyncServerStreamingCall<RouteMessage> _receive;
        public SubscribeTools<EventData<RTCSessionDescription>> OnOffer { get; } 
            =new SubscribeTools<EventData<RTCSessionDescription>>();

        public SubscribeTools<EventData<RTCSessionDescription>> OnAnswer { get; } 
            =new SubscribeTools<EventData<RTCSessionDescription>>();

        public SubscribeTools<EventData<RTCIceCandidate>> OnIceCandidate { get; } 
            = new SubscribeTools<EventData<RTCIceCandidate>>();

        public SubscribeTools<string> OnPeerOnline { get; }
            = new SubscribeTools<string>();

        public SubscribeTools<string> OnPeerOffline { get; }
            = new SubscribeTools<string>();

        public SubscribeTools<string> OnDisconnect { get; }
            = new SubscribeTools<string>();

        public string SessionId => _sessionId;
      

        public SignalChannel(string address, TokenSession session):this(address.Split(':')[0],
            int.Parse(address.Split(':')[1]), session)
        {
            
        }

        public SignalChannel(string ip, int port, TokenSession session)
        {
            _ip = ip;
            _port = port;
            _sessionId = session.AccountId;
            _tokenSession = session;
        }
        
        private async Task HandleReceive(RouteMessage t)
        {
            try
            {
                Debugger.LogIfLevel(LoggerType.Debug, () => $"Received:{t}");
                if (t.Msg.TryUnpack<CreateOffer>(out var offer))
                {
                    await UniTask.SwitchToMainThread();
                    Debugger.LogIfLevel(LoggerType.Debug,()=>$"OfferDes:{offer.Description}");
                    var data = new EventData<RTCSessionDescription>()
                    {
                        FromID =  t.FromId,
                        Data = JsonUtility.FromJson<RTCSessionDescription>(offer.Description)
                    };
                    OnOffer?.Invoke(  data);
                    return;
                }

                if (t.Msg.TryUnpack<AnswerOffer>(out var answer))
                {
                    await UniTask.SwitchToMainThread();
                    Debugger.LogIfLevel(LoggerType.Debug,()=>$"Answer:{answer.Description}");
                    var data = new EventData<RTCSessionDescription>()
                    {
                        FromID =  t.FromId,
                        Data = JsonUtility.FromJson<RTCSessionDescription>(answer.Description)
                    };
                    OnAnswer?.Invoke(data);
                }

                if (t.Msg.TryUnpack<IceCandidate>(out var candidate))
                {
                    await UniTask.SwitchToMainThread();
                    Debugger.LogIfLevel(LoggerType.Debug,()=>$"candidate:{candidate.IceCandidate_}");
                    var data = new EventData<RTCIceCandidate>
                    {
                        Data = JsonUtility.FromJson<RTCIceCandidate>(candidate.IceCandidate_),
                        FromID = t.FromId
                    };
                    OnIceCandidate?.Invoke(data);
                }

                if (t.Msg.TryUnpack<PeerOnline>(out var online))
                {
                    await UniTask.SwitchToMainThread();
                    OnPeerOnline?.Invoke(online.SessionId);
                }

                if (t.Msg.TryUnpack(out PeerOffLine peerOffLine))
                {
                    await UniTask.SwitchToMainThread();
                    OnPeerOffline?.Invoke(peerOffLine.SessionId);
                }
            }
            catch (Exception ex)
            {
                Debugger.LogIfLevel(LoggerType.Error,()=>ex.ToString());
            }
        }
        
        public async Task ConnectAsync(CancellationToken token = default)
        {
            _logChannel = new LogChannel(_ip, _port);
            await _logChannel.ConnectAsync(DateTime.UtcNow.AddSeconds(10));
            _service = _logChannel.CreateClient<SignalServerService.SignalServerServiceClient>();
            var linked = CancellationTokenSource.CreateLinkedTokenSource(_logChannel.ShutdownToken, token);

            _receive = _service.Connect(new C2S_Connect { Session = _tokenSession},
                cancellationToken: linked.Token);

            _ = Task.Factory.StartNew(async () =>
            {
                var error = string.Empty;
                try
                {
                    await _receive.ResponseStream.ForEachAsync(HandleReceive);
                }
                catch(Exception ex)
                {
                    //ignore
                    //Debugger.LogIfLevel(LoggerType.Debug,()=>ex.ToString());
                    error = ex.Message;
                }

                await UniTask.SwitchToMainThread();
                OnDisconnect.Invoke(error);
                Debugger.LogIfLevel(LoggerType.Debug,()=>$"Exit:{_sessionId}");
            }, linked.Token);
        }
        
        public async Task Route<T>(string to, T msg) where  T:IMessage
        {
            await _service.RouteAsync(new RouteMessage()
            {
                FromId = _sessionId,
                ToId = to,
                Msg = Any.Pack(msg)
            });
        }
        
        
        public async Task<IList<string>> QueryPlayers()
        {
            var result = await _service.QueryPlayerListAsync(new C2S_QueryPlayerList {ConnectionId = _sessionId},
                cancellationToken: _logChannel.ShutdownToken);
            return result.Playies;
        }

        public void Dispose()
        {
            _receive?.Dispose();
            OnOffer?.Dispose();
            OnAnswer?.Dispose();
            OnIceCandidate?.Dispose();
            OnPeerOnline?.Dispose();
            OnPeerOffline?.Dispose();
            OnDisconnect?.Dispose();
            _ = _logChannel?.ShutdownAsync();
        }
    }

}