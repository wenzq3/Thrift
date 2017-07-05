using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server.Config;
using System.Threading;
using org.apache.zookeeper;
using static org.apache.zookeeper.Watcher.Event;
using org.apache.zookeeper.data;
using System.Diagnostics;

namespace Thrift.Server
{
    /// <summary>
    /// 自动注册服务
    /// </summary>
    public class RegeditConfig : Watcher
    {
        private Service _service;
        private string _host;
        private ZooKeeper _zk;
        private List<ACL> _zk_Acl;

        private static object lockHelp = new object();

        public RegeditConfig(Service service)
        {
            ZooKeeper.CustomLogConsumer = new ZookeeperLog();
            ZooKeeper.LogToFile = false;
            ZooKeeper.LogToTrace = false;

            _zk = null;
            _service = service;
            _zk_Acl = ZooDefs.Ids.OPEN_ACL_UNSAFE;

            _host = _service.Host;
            if (string.IsNullOrEmpty(_host))
            {
                _host = GetAddressIP();
                if (_host == "")
                    throw new Exception("当前服务IP地址不能为空");
            }
        }

        public void Logout()
        {
            if (_zk != null)
            {
                _zk.closeAsync();
                _zk = null;
            }
        }

        public void Start()
        {
            while (true)
            {
                if (Regedit())
                    break;

                System.Threading.Thread.Sleep(_service.ZookeeperConfig.ConnectInterval);
            }
        }

        private bool Regedit()
        {
            if (_zk == null)
            {
                _zk = new ZooKeeper(_service.ZookeeperConfig.Host, _service.ZookeeperConfig.SessionTimeout, this);

                int count = 0;
                while (_zk.getState() != ZooKeeper.States.CONNECTED)
                {
                    if (count++ > 50)
                        throw new Exception("ZooKeeper 连接失败");
                    System.Threading.Thread.Sleep(100);
                }
            }

            var zNode = $"{_service.ZookeeperConfig.NodeParent}/{_host}:{_service.Port}-{_service.Weight}";

            if (!CheckNodeParent())
                return false;

            if (!RegeditNode(zNode, 0))
                return false;

            return true;
        }

        /// <summary>
        /// 创建父结点
        /// </summary>
        private bool CheckNodeParent()
        {
            string[] list = _service.ZookeeperConfig.NodeParent.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < list.Count(); i++)
            {
                string zNode = "";
                for (int j = 0; j <= i; j++)
                    zNode += "/" + list[j];

                try
                {
                    var stat = _zk.existsAsync(zNode).Result;
                    if (stat == null)
                    {
                        if (zNode == "/ThriftServer") //thrift服务的根目录
                            _zk.createAsync(zNode, new byte[0] { }, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                        else
                            _zk.createAsync(zNode, new byte[0] { }, _zk_Acl, CreateMode.PERSISTENT);
                    }
                }
                catch (Exception ex)
                {
                    ThriftLog.Error("zk:创建父结点异常:" + ex.Message + ex.StackTrace);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///  注册服务直到成功
        ///  具体RPC服务的注册路径为: /thrift/{namespace}, 该路径上的节点都是永久节点
        ///  RPC服务集群节点的注册路径为: /thrift/{namespace}/ip:port:weight, 末尾的节点是临时节点.
        /// </summary>
        private bool RegeditNode(string zNode, int existsCount)
        {
            try
            {
                var r = _zk.createAsync(zNode, new byte[0] { }, _zk_Acl, CreateMode.EPHEMERAL).Result;
                ThriftLog.Info($"{zNode}节点注册完成");
                return true;
            }
            catch (Exception ex)
            {
                ThriftLog.Info($"{zNode}节点注册失败：" + ex.StackTrace + ex.Message);
                return false;
            }
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            Console.WriteLine("WatchedEvent:" + watchedEvent.getState().ToString() + ":" + watchedEvent.get_Type().ToString());
            if (watchedEvent.getState() == KeeperState.Expired)
            {
                ThriftLog.Info("重新注册zk");
                await _zk.closeAsync();
                _zk = null;
                Start();
            }
            return;
        }

        /// <summary>
        /// 获取本地IP地址信息
        /// </summary>
        private string GetAddressIP()
        {
            List<string> ips = new List<string>();
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    if (_IPAddress.ToString().IndexOf("10.") == 0) return _IPAddress.ToString();
                    if (_IPAddress.ToString().IndexOf("192.") == 0) return _IPAddress.ToString();
                    ips.Add(_IPAddress.ToString());
                }
            }
            if (ips.Count > 0) return ips[0];

            return "";
        }
    }
}
