using CommandLine;

namespace SessionServer
{
    public class SessionOption
    {
        [Option("port", Default = 1200)]
        public int Port { set; get; }
        
        [Option("ip", Default = "0.0.0.0")]
        public string Address { set; get; }
        
        [Option("dbstr")]
        public string DbStr { set; get; }
        [Option("dbname")]
        public string DbName { set; get; }
        [Option("zk")]
        public string ZkServers { set; get; }
        
        [Option("zkroot")]
        public string ZkRoot { set; get; }
    }
}