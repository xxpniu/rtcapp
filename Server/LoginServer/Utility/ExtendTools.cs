using LoginServer.DataBase.Entities;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.LoginServer;

namespace LoginServer.Utility
{
    public static class ExtendTools
    {
        public static TokenSession ToTokenSession(this Session session)
        {
            return new() {Token = session.Token, AccountId = session.AccountId};
        }
    }
}