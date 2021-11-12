using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

namespace XRTC
{
    public class RTCClient : IDisposable
    {
        public bool IsOfferClient { set; get; }
        public RTCPeerConnection PeerConnection { get; set; }
        public Texture PeerTexture { set; get; }

        public AudioClip Clip { set; get; }
        public string Name { get; set; }
        public RTCRtpSender VideoTrack { get; set; }
        public RTCRtpSender AudioTrack { get; set; }

        public void Dispose()
        {
            PeerConnection?.Dispose();
            if (PeerTexture != null) UnityEngine.Object.Destroy(PeerTexture);
            PeerTexture = null;
            PeerConnection = null;
            GC.SuppressFinalize(this);
        }

        public void SetTexture(Texture texture)
        {
            if (PeerTexture != null) UnityEngine.Object.Destroy(PeerTexture);
            PeerTexture = texture;
        }

        public async Task SetRemoteDescription(RTCSessionDescription desData)
        {
            await UniTask.SwitchToMainThread();
            var op = PeerConnection.SetRemoteDescription(ref desData);
            await op;
            if (op.IsError) throw new Exception($"set remote error: {op.Error.message}");
        }

        public void RemoveVideoTrack()
        {
            if (VideoTrack == null) return;
            PeerConnection.RemoveTrack(VideoTrack);
            VideoTrack.Dispose();
            VideoTrack = null;
        }

        public void RemoveAudioTrack()
        {
            if (AudioTrack == null) return;
            PeerConnection.RemoveTrack(AudioTrack);
            AudioTrack.Dispose();
            AudioTrack = null;
        }

        public void SetAudioClip(AudioClip renderer)
        {
            Clip = renderer;
        }

        ~RTCClient()
        {
            Dispose();
            
        }
    }

}