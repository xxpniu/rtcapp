using System.Threading.Tasks;
using CommandLine;
using Grpc.Core;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.SignalServer;
using RTC.ServerUtility;
using RTC.XNet;

namespace SignalServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var option = new SignalOption()
            {
                ListenPort = 3380,
                Address = "127.0.0.1"
            };
            Parser.Default.ParseArguments<SignalOption>(args)
                .WithParsed(o => { option = o; });

            var log = new  DefaultLogger();
   
            Debugger.Logger = log ;
            
            GrpcEnvironment.SetLogger(log);
            Debugger.LogIfLevel(LoggerType.Debug, ()=> $"{args.JoinToString("\n")}");
            
            var server = new LogServer()
            {
                Ports = {new ServerPort("0.0.0.0", option.ListenPort, ServerCredentials.Insecure)}
            };


            //zk health check
            var zk = new ZkHealthy<ServerDefine>(option.ZkRoot,
                $"{option.Address}:{option.ListenPort}",
                option.ZkServers,
                new ServerDefine
                {
                    Address = option.Address,
                    Port = option.ListenPort
                });

            //watcher server sso
            var watcher = new WatcherServer<string, ServerDefine>(zk.ZooKeeper, option.ZkSSOServer,
                define => $"{define.Address}:{define.Port}");
            var service = new GrpcHandler.SignalService(watcher);
            
            server.BindServices(SignalServerService.BindService(service));
            //watcher.GetEnumerator()

            await App.Setup(async (a) =>
            {
                server.Start();
                await zk.CreateAsync();
                await watcher.RefreshData();
                await Task.CompletedTask;
            }, async (v) =>
            {
                await zk.CloseAsync();
                await server.ShutdownAsync();
            }).RunAsync();
        }
    }
}