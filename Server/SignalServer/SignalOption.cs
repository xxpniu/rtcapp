using CommandLine;

namespace SignalServer
{
    public class SignalOption
    {
        [Option('p',"port"  , Default = 3400  )]
        public int ListenPort { set; get; }
        
        [Option("ip", Default = "0.0.0.0")]
        public string Address { set; get; }
        
        [Option("zk")]
        public string ZkServers { set; get; }

        [Option("zksso", Default = "sso")]
        public string ZkSSOServer { set; get; }
        
          
        [Option("zkroot")]
        public string ZkRoot { set; get; }
    } 
}