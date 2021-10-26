using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using org.apache.zookeeper;
using RTC.XNet;

namespace RTC.ServerUtility
{
    public class ZkHealthy<T> where T:IMessage 
    {
        public string Root { set; get; }
        public string ServerId { set; get; }
        
        public  T Data { set; get; }

        private ZooKeeper _zk;

        public ZooKeeper ZooKeeper => _zk;
         
        
        public ZkHealthy(string root, string serverId, string zkHost, T data)
        {
            _zk = new ZooKeeper(zkHost,3000, new DefaultWatcher());
            Root = root;
            ServerId = serverId;
            Data = data;
        }

        public async Task<bool> CreateAsync()
        {
            if (await _zk.existsAsync(Root) == null)
            {
                await _zk.createAsync(Root, new byte[] {0}, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                Debugger.LogIfLevel(LoggerType.Debug, () => $"Created Root:{Root}");
            }

            var path = $"{Root}/{ServerId}";
            if (await _zk.existsAsync(path) != null)
            {
                throw new KeeperException.NodeExistsException($"Path existed:{path}");
            }

            await _zk.createAsync(path, Encoding.UTF8.GetBytes(Data.ToJson()), ZooDefs.Ids.OPEN_ACL_UNSAFE,
                CreateMode.EPHEMERAL);
            
            Debugger.LogIfLevel(LoggerType.Debug,()=>$"Add Node:{path} - {Data}");
            return true;
        }

        public async Task CloseAsync()
        {
          await  _zk.closeAsync();
        }
    }
}