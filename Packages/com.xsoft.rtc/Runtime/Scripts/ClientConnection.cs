using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.SignalServer;
using RTC.XNet;
using Unity.WebRTC;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace XRTC
{
    public class RTCClientConnection: IDisposable
    {
        private CancellationTokenSource _source = new CancellationTokenSource();
       
        private SignalChannel Signal { get; set; }
        private Dictionary<string, RTCClient> Connections { get; } =
            new Dictionary<string, RTCClient>();

        public SubscribeTools<(bool add, string id)> OnPeerChanged { get; }
            = new SubscribeTools<(bool add, string id)>();

        public SubscribeTools<string> OnOffline { get; }
            = new SubscribeTools<string>();
        

        private WebCamTexture webCamTexture;
        private Vector2Int Size { get; }

        private RTCConfiguration config;// { get; }

        private async Task<AudioClip> CreateMicrophoneAsync(CancellationToken token = default)
        {
            var m_deviceName = Microphone.devices.FirstOrDefault();
            var m_clipInput = Microphone.Start(m_deviceName, true, 1, 48000);
            // set the latency to “0” samples before the audio starts to play.
            Debug.Log($" microphone {m_deviceName}");
            await UniTask.WaitUntil(() => Microphone.GetPosition(m_deviceName) > 0, cancellationToken:token);
            
            var source = _audioSource.AddComponent<AudioSource>();
            source.clip = m_clipInput;
            source.loop = true;
            source.mute = true;
            source.Play();
            MicrophoneClip = source;
            return m_clipInput;
        }

        private async Task CreateWebcameraTextureAsync(bool front = true, CancellationToken token = default)
        {
            await UniTask.SwitchToMainThread();
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                Object.Destroy(webCamTexture);
            }
            if(WebCamTexture.devices.Length == 0) throw new Exception($"Not found camera!");
            var device = "";
            foreach (var target in WebCamTexture.devices)
            {
                if (target.isFrontFacing != front) continue;
                device = target.name;
                break;
            }
            
            //if(device)

            if (string.IsNullOrEmpty(device))
            {
                device = WebCamTexture.devices.First().name;
            }

            webCamTexture = new WebCamTexture(device, Size.x, Size.y, 30);
            webCamTexture.Play(); 
            
            //WebCamTexture.devices.First().kind
            
            await UniTask.WaitUntil(() => webCamTexture.didUpdateThisFrame, cancellationToken: token);
        }

        public async void ChangeCamera()
        {
            if (videoTrack != null)
            {
                foreach (var ky in Clients)
                {
                     ky.Value.RemoveVideoTrack();
                }
            }
        }

        public RTCClientConnection(TokenSession session, string signalAddress, string stunAddress,
            Vector2Int? size = default)
        {
            if (size.HasValue) Size = size.Value;
            config = new RTCConfiguration
            {
                iceServers = new[] {new RTCIceServer() {urls = new[] {stunAddress}}}
            };
            Init(session,signalAddress);
        }

        private void Init(TokenSession session, string signalAddress)
        {
            Signal = new SignalChannel(signalAddress, session);
            Signal.OnPeerOnline.Subscribe((des =>
            {
                if (!Connections.TryGetValue(des, out var peerConnection)) return false;
                Debugger.LogError($"Existed player {des}");
                return false;
            }));

            Signal.OnPeerOffline.Subscribe(id =>
            {
                if (!Connections.TryGetValue(id, out var peerConnection)) return false;
                peerConnection?.Dispose();
                Connections.Remove(id);
                OnPeerChanged.Invoke((false,id));
                return false;
            });
            
            Signal.OnOffer.Subscribe(des =>
            {
                _ = InitFromAnswerOfferAsync(remoteId: des.FromID, des.Data);
                //on other peer create offer
                return false;
            });
            
            Signal.OnIceCandidate.Subscribe(des =>
            {
                if (Connections.TryGetValue(des.FromID, out var client))
                {
                    client.PeerConnection?.AddIceCandidate(des.Data);
                }
                return false;
            });
            
            Signal.OnDisconnect.Subscribe(des =>
            {
                OnOffline.Invoke(des);
                return false;
            });
        }

        public Dictionary<string, RTCClient> Clients => Connections;

        public WebCamTexture WebCam => webCamTexture;
        public AudioSource MicrophoneClip { private set; get; } 
        public async Task ConnectAsync(bool front = true, CancellationToken token = default)
        {
            _audioSource = new GameObject("RTCClient");
             
            await CreateMicrophoneAsync(token: token);
            await CreateWebcameraTextureAsync(front, token: token);
            await Signal.ConnectAsync(token);
            var onlinePlayers = await Signal.QueryPlayers();

            foreach (var player in onlinePlayers)
            {
                if(player == Signal.SessionId) continue;
                if (Connections.TryGetValue(player, out _)) continue;
                try
                {
                    var source = new CancellationTokenSource();
                    source.CancelAfter(TimeSpan.FromSeconds(20));
                    await InitFromCreateOfferAsync(player, source.Token);
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }

            }
        }
        
        private RTCClient CreateClient(string remoteId)
        {
            if (Connections.TryGetValue(remoteId, out var client))
            {
                client?.Dispose();
                Connections.Remove(remoteId);
            }

            var peer = new RTCPeerConnection(ref config)
            {
                
                OnIceCandidate = ice =>
                {
                    if(string.IsNullOrEmpty(ice.Candidate)) return;
                    _ = Signal.Route(remoteId, new IceCandidate {IceCandidate_ = JsonUtility.ToJson(ice)});
                },
               
                OnNegotiationNeeded = () => { Debugger.LogIfLevel(LoggerType.Debug, () => $"OnNegotiationNeeded()"); },
                OnDataChannel = (channel) =>
                {
                    // _dataChannel = channel;
                    channel.OnMessage = (msg) =>
                    {
                        Debug.LogError("PollClientLogBytesLength: " + msg.Length);
                        var any = Any.Parser.ParseFrom(msg);
                        Debugger.Log(any);
                    };
                    //remoteDataChannel = channel;
                    //remoteDataChannel.OnMessage = onDataChannelMessage;
                }
            };
            
            
            videoTrack = new VideoStreamTrack(webCamTexture);
            audioTrack = new AudioStreamTrack(MicrophoneClip);


            client = new RTCClient()
            {
                PeerConnection = peer,
                VideoTrack = peer.AddTrack(videoTrack),
                AudioTrack = peer.AddTrack(audioTrack)
            };
            
            Connections[remoteId] = client;
            peer.OnConnectionStateChange = state =>
            {
                Debugger.LogIfLevel(LoggerType.Debug, () => $"State:{state.ToString()}");
                switch (state)
                {
                    case RTCPeerConnectionState.Disconnected:
                        if (client.IsOfferClient)
                        {
                            _ = InitFromCreateOfferAsync(remoteId);
                        }
                        break;
                    case  RTCPeerConnectionState.Failed:
                        OnOffline.Invoke("failure");
                        break;
                }
            };

            peer.OnIceConnectionChange = state =>
            {
                Debugger.LogIfLevel(LoggerType.Debug, () => $"OnIceConnectionChange State:{state.ToString()}");
                switch (state)
                {
                    case   RTCIceConnectionState.Closed:
                        break;
                }
            };
            peer.OnTrack = evnt =>
            {
                Debugger.LogIfLevel(LoggerType.Debug, () => $"Add Track:{evnt.Track}");
                if (!Connections.TryGetValue(remoteId, out var rtcClient)) return;
                switch (evnt.Track)
                {
                    case AudioStreamTrack audioStreamTrack:
                        //var outputAudioSource = receiveObjectList[audioIndex];
                        audioStreamTrack.OnAudioReceived += clip =>
                        {
                            rtcClient.SetAudioClip(clip);
                        };
                        break;
                    case VideoStreamTrack video:
                    {
                        if(video.IsDecoderInitialized) return;
                        var texture = video.InitializeReceiver(Size.x, Size.y);
                        rtcClient.SetTexture(texture);

                        var st = evnt.Streams.FirstOrDefault();
                        if (st == null) return;

                        st.OnRemoveTrack = ev =>
                        {
                            Debugger.LogIfLevel(LoggerType.Debug, () => $"Un track");
                            texture = null;
                            rtcClient.SetTexture(null);
                            ev.Track.Dispose();
                        };
                        Debugger.LogIfLevel(LoggerType.Debug, () => $" video :{video.Id}");
                        break;
                    }
                }
            };
            return client;
        }

        private GameObject _audioSource;

        private VideoStreamTrack videoTrack;
        private AudioStreamTrack audioTrack;

        private async Task InitFromAnswerOfferAsync(string remoteId, RTCSessionDescription des,CancellationToken token = default)
        {
            try
            {
                var client = CreateClient(remoteId);
                client.IsOfferClient = false;
                var offerDes = new RTCSessionDescription
                {
                    type = RTCSdpType.Offer,
                    sdp = des.sdp
                };

                Debugger.Log("AnswerOfferAsync Call");
                await client.SetRemoteDescription(offerDes);
                var aOp = client.PeerConnection.CreateAnswer();
                await aOp;
                if (aOp.IsError) throw new Exception($"op:{aOp.Error.message}");
                var sOp = client.PeerConnection.SetLocalDescription();
                await sOp;
                if (sOp.IsError) throw new Exception($"op:{sOp.Error.message}");
                await Signal.Route(remoteId, new AnswerOffer
                {
                    ReceiveConnectionId = remoteId,
                    Description = JsonUtility.ToJson(client.PeerConnection.LocalDescription)
                });
                OnPeerChanged.Invoke((true,remoteId));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private async Task InitFromCreateOfferAsync(string remoteId, CancellationToken token = default)
        {
            try
            {
                var client = CreateClient(remoteId);
                client.IsOfferClient = true;
                var op1 = client.PeerConnection.CreateOffer();
                await op1;
                if (op1.IsError) throw new Exception($"Create offer to remote {remoteId} {op1.Error.message}");
                var op2 = client.PeerConnection.SetLocalDescription();
                await op2;
                if (op2.IsError) throw new Exception($"set local description of {remoteId} {op2.Error.message}");

                var waitingAnswer = true;
                Signal.OnAnswer.Subscribe((des) =>
                {
                    if (!string.Equals(des.FromID, remoteId)) return false;
                    _ = client.SetRemoteDescription(des.Data);
                    waitingAnswer = false;
                    return true;
                });

                await Signal.Route(remoteId, new CreateOffer
                {
                    ReceiveConnectionId = remoteId,
                    Description = JsonUtility.ToJson(client.PeerConnection.LocalDescription)
                });

                await UniTask.WaitUntil(() => !waitingAnswer, cancellationToken: token);
                OnPeerChanged.Invoke((true,remoteId));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        

        public void Dispose()
        {
            OnOffline?.Dispose();
            OnPeerChanged?.Dispose();   
            videoTrack?.Dispose();
            audioTrack?.Dispose();
            Signal?.Dispose();
            Object.Destroy(_audioSource);
            //_= Signal.CloseAsync();
            foreach (var kv in Connections)
            {
                kv.Value.Dispose();
            }
            _source.Cancel();
        }
    }

}