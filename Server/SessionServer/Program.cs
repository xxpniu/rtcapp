using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using LoginServer.DataBase;
using SessionServer.GrpcHandler;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.SessionServer;
using RTC.ServerUtility;
using RTC.XNet;

namespace SessionServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var option = new SessionOption()
            {
                Port = 3380,
                Address = "127.0.0.1"
            };
            Parser.Default.ParseArguments<SessionOption>(args)
                .WithParsed(o => { option = o; });

            var log = new  DefaultLogger();
   
            Debugger.Logger = log ;
            
            GrpcEnvironment.SetLogger(log);
            
            Debugger.LogIfLevel(LoggerType.Debug, ()=> $"{args.JoinToString("\n")}");
            
            var server = new LogServer()
            {
                Ports = {new ServerPort("0.0.0.0", option.Port, ServerCredentials.Insecure)}
            };

            var service = new SessionServiceImp();

            server.BindServices(SessionService.BindService(service));

            var zk = new ZkHealthy<ServerDefine>(option.ZkRoot,
                $"{option.Address}:{option.Port}", 
                option.ZkServers, new ServerDefine
            {
                Address = option.Address,
                Port = option.Port
            });
            
            

            await App.Setup(
                    async _ =>
                    {
                        await DataBaseManager.S.InitAsync(option.DbStr, option.DbName);
                        server.Start();
                        await zk.CreateAsync();
                    },
                async _ =>
                    {
                        await server.ShutdownAsync();
                        await zk.CloseAsync();
                    })
                .RunAsync();
        }
    }
}