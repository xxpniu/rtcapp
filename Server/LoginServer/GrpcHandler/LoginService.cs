using System;
using System.Threading.Tasks;
using Grpc.Core;
using LoginServer.DataBase;
using LoginServer.Utility;
using RTC.ProtoGrpc.LoginServer;
using RTC.XNet;

namespace LoginServer.GrpcHandler
{
    public class LoginService : LoginServerService.LoginServerServiceBase
    {
        private readonly LogServer _server;

        public LoginService(LogServer server)
        {
            _server = server;
        }

        private async Task<bool> WriteSessionAsync(ServerCallContext context, string accountId)
        {
            if (!(await context.WriteSession(accountId, _server)))
                throw new Exception("Error");
            return true;
        }

        [Auth]
        public override async Task<L2C_Login> Login(C2L_Login request, ServerCallContext context)
        {
            var session =
                await DataBaseManager.S.LoginAsync(request.UserName, request.Password, context.CancellationToken);
            //session error 
            if (session == null)
            {
                (_, session) = await DataBaseManager.S.CreateAccountAsync(request.UserName, request.Password,
                    string.Empty
                    , string.Empty,
                    context.CancellationToken);
            }

            if (session == null)
            {
                throw new ResultException($"{request} is error with password");
            }

            await WriteSessionAsync(context, session.AccountId);

            return new L2C_Login
            {
                Token = session.ToTokenSession()
            };
        }

        [Auth]
        public override async Task<L2C_RegisterAccount> Register(C2L_RegisterAccount request, ServerCallContext context)
        {
            var (_, session) = await DataBaseManager.S.CreateAccountAsync(request.UserName, request.Password,
                request.Nickname, request.Email,
                context.CancellationToken);

            if (! await WriteSessionAsync(context, session.AccountId))
            {
                //debugger log error
            }

            return new L2C_RegisterAccount
            {
                Token = session.ToTokenSession()
            };
        }
    }
}