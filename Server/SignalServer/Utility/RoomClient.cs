using System;
using RTC.ProtoGrpc.SignalServer;
using RTC.ServerUtility;

namespace SignalServer.Utility
{
    public class RoomClient: IDisposable
    {
        public AsyncStreamBuffer<RouteMessage> Buffer { get; }
        
        public string RoomId {  get; }

        public RoomClient(string roomId, AsyncStreamBuffer<RouteMessage> buffer)
        {
            Buffer = buffer;
            RoomId = roomId;
        }

        public void Dispose()
        {
            Buffer?.Dispose();
        }

        public void Push(RouteMessage request)
        {
            Buffer.Push(request);
        }
    }
}