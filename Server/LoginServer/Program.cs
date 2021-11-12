using System;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using MongoDB.Bson;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.LoginServer;
using RTC.ServerUtility;
using RTC.XNet;
using Debugger = RTC.XNet.Debugger;

namespace LoginServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {

            var option = new LoginOption();
            var log = new  DefaultLogger();
   
            Debugger.Logger = log.WriteLog ;
            
            GrpcEnvironment.SetLogger(log);

            Parser.Default.ParseArguments<LoginOption>(args)
                .WithParsed(o => { option = o; });
            
            Debugger.LogIfLevel(LoggerType.Debug, ()=> $"{args.JoinToString("\n")}");

            var server = new LogServer
            {
                Ports = {new ServerPort("0.0.0.0", option.Port, ServerCredentials.Insecure)}
            };

            var zk = new ZkHealthy<ServerDefine>(option.ZkRoot,
                $"{option.Address}:{option.Port}",
                option.ZkServers, new ServerDefine
                {
                    Address = option.Address,
                    Port = option.Port
                });

            var handler = new GrpcHandler.LoginService(server);
            server.BindServices(LoginServerService.BindService(handler));

            //server.Interceptor.SetAuthCheck((context => server.CheckSession(context.GetAccountId(), out _)));

            await App.Setup(async (go) =>
            {
                await DataBase.DataBaseManager.S.InitAsync(option.DbStr, option.DbName);
                server.Start();
                await zk.CreateAsync();
            }, async (_) =>
            {
                await zk.CloseAsync();
                await server.ShutdownAsync();
                await Task.CompletedTask;
            }).RunAsync();
        }
    }
}