using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using RTC.ProtoGrpc.SignalServer;

namespace SignalServer.Utility
{
    public static class Extends
    {
        public static RouteMessage ToRoute(this IMessage msg,string from, string to)
        {
            return new RouteMessage
            {
                FromId = from,
                ToId = to,
                Msg = Any.Pack(msg)
            };
        }
    }
}