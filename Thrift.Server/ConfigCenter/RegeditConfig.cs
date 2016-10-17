using Org.Apache.Zookeeper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZooKeeperNet;
using Thrift.Server.Config;
using System.Threading;

namespace Thrift.Server
{
    /// <summary>
    /// 自动注册服务
    /// </summary>
    public class RegeditConfig : IWatcher
    {
        private Service _service;
        private string _host;
        private ZooKeeper _zk;
        private List<ACL> _zk_Acl;

        private bool _isLogout; //是否注销中

        Thread _threadStart;

        private static object lockHelp = new object();

        public RegeditConfig(Service service)
        {
            _zk = null;
            _isLogout = false;
            _service = service;
            _zk_Acl = Ids.OPEN_ACL_UNSAFE;

            _host = _service.Host;
            if (string.IsNullOrEmpty(_host))
            {
                _host = GetAddressIP();
                if (_host == "")
                    throw new Exception("当前服务IP地址不能为空");
            }
            _threadStart = null;
        }

        public void Logout()
        {
            _isLogout = true;
            if (_zk != null)
                _zk.Dispose();
        }

        public void ReStart()
        {
            lock (lockHelp)
            {
                if (_zk != null)
                {
                    _zk = null;
                    //_zk.Dispose();
                    System.GC.Collect();
                }
                if (_threadStart != null)
                {
                    _threadStart.Abort();
                    _threadStart = null;
                }
                Start();
            }
        }

        public void Start()
        {
            _threadStart = new Thread(() =>
        {
            while (true)
            {
                if (_isLogout) break;

                if (Regedit())
                    break;

                System.Threading.Thread.Sleep(_service.ZookeeperConfig.ConnectInterval);
            }
        });
            _threadStart.Start();
        }

        private bool Regedit()
        {
            _zk = ZookeeperHelp.CreateClient(_service.ZookeeperConfig.Host, _service.ZookeeperConfig.SessionTimeout, this, _service.ZookeeperConfig.Digest);

            if (_zk == null)
            {
                ThriftLog.Info($"Zookeeper服务 {_service.ZookeeperConfig.Host} 连接失败");
                return false;
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
                    var stat = _zk.Exists(zNode, true);
                    if (stat == null)
                    {
                        if (zNode == "/ThriftServer") //thrift服务的根目录
                            _zk.Create(zNode, new byte[0] { }, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                        else
                            _zk.Create(zNode, new byte[0] { }, _zk_Acl, CreateMode.Persistent);
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
                _zk.Create(zNode, new byte[0] { }, _zk_Acl, CreateMode.Ephemeral);
                ThriftLog.Info($"{zNode}节点注册完成 ");
                return true;
            }
            catch (KeeperException.NodeExistsException ex)
            {
                ThriftLog.Info($"{zNode}节点已经存在 " + ex.Message);
                System.Threading.Thread.Sleep(2000);
                if (existsCount++ > 5)
                {
                    ThriftLog.Info($"{zNode}节点注册完成 ");
                    return true;
                }
                else
                    return RegeditNode(zNode, existsCount);
            }
            catch (KeeperException.SessionExpiredException ex)
            {
                ThriftLog.Info("SessionExpiredException 过期" + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                ThriftLog.Info("Regedit:" + ex.StackTrace);
                return false;
            }
        }

        public void Process(WatchedEvent @event)
        {
            ThriftLog.Info("WatchedEvent :" + @event.State.ToString());
            //     if (@event.State == KeeperState.Disconnected || @event.State == KeeperState.Expired)
            if (@event.State == KeeperState.Expired)
            {
                if (_isLogout) return;
                ThriftLog.Info(" 重新注册zk");
                ReStart();
            }
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
                    if (_IPAddress.ToString().IndexOf("192.") == 0) return _IPAddress.ToString();
                    ips.Add(_IPAddress.ToString());
                }
            }
            if (ips.Count > 0) return ips[0];

            return "";
        }
    }
}
