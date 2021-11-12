using RTC.ProtoGrpc.Data;

namespace RTC.ServerUtility
{
    public static  class Extends
    {
        public static string ToEndPoint(this ServerDefine server)
        {
            return $"{server.Address}:{server.Port}";
        }
        
      
    }
}