using System.Threading.Tasks;
using Grpc.Core;
using LoginServer.DataBase;
using RTC.ProtoGrpc.SessionServer;

namespace SessionServer.GrpcHandler
{
    public class SessionServiceImp: SessionService.SessionServiceBase
    {
        public override async Task<S2O_CheckSession> CheckSession(O2S_CheckSession request, ServerCallContext context)
        {
            var result =
              await  DataBaseManager.S.CheckSessionAsync(request.AccountId, request.Token, context.CancellationToken);
            return new S2O_CheckSession
            {
                Available = result,
                Message = $"Result:{request}"
            };
        }
    }
}