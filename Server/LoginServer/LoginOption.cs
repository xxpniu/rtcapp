using CommandLine;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace LoginServer
{
    public class LoginOption
    {
        [Option("dbstr")]
        public string DbStr { set; get; }
        [Option("dbname")]
        public string DbName { set; get; }
        [Option("port")]
        public int Port { set; get; }
        
        [Option("ip")]
        public string Address { set; get; }
        
        [Option("zkroot", Default = "/loginserver")]
        public string ZkRoot { set; get; }
        
        [Option("zk")]
        public string ZkServers { set; get; }
    }
}