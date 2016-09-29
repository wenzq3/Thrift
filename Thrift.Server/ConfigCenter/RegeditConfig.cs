using Org.Apache.Zookeeper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZooKeeperNet;
using Thrift.Server.Config;

namespace Thrift.Server
{
    /// <summary>
    /// 自动注册服务
    /// </summary>
    public class RegeditConfig : IWatcher
    {
        private Service _service;

        private ZooKeeper _zk;
        private List<ACL> _zk_Acl;

        private bool _isLogout; //是否注销中


        public RegeditConfig(Service service)
        {
            _zk = null;
            _isLogout = false;
            _service = service;

            //  if (string.IsNullOrEmpty(_service.ZookeeperConfig.Digest))
            _zk_Acl = Ids.OPEN_ACL_UNSAFE;
            //    else
            //        _zk_Acl = ZookeeperHelp.GetACL(_service.ZookeeperConfig.Digest);
        }

        private bool Regedit()
        {
            _zk = ZookeeperHelp.CreateClient(_service.ZookeeperConfig.Host, _service.ZookeeperConfig.SessionTimeout, this, _service.ZookeeperConfig.Digest);

            if (_zk == null)
            {
                ThriftLog.Info($"Zookeeper服务 {_service.ZookeeperConfig.Host} 连接失败");
                return false;
            }

            string _host = _service.Host;
            if (string.IsNullOrEmpty(_host))
            {
                _host = GetAddressIP();
                if (_host == "")
                    throw new Exception("当前服务IP地址不能为空");
            }
            var zNode = $"{_service.ZookeeperConfig.NodeParent}/{_host}:{_service.Port}-{_service.Weight}";

            new System.Threading.Thread(() =>
            {
                CheckNodeParent();
                Regedit(zNode);
            }).Start();

            return true;
        }

        public void Start()
        {
            _zk = null;

            bool isConnZk = Regedit();
            if (!isConnZk)
            {
                new System.Threading.Thread(() =>
                {
                    while (!isConnZk)
                    {
                        System.Threading.Thread.Sleep(_service.ZookeeperConfig.ConnectInterval);
                        isConnZk = Regedit();
                    }
                }).Start();
            }
        }

        public void Logout()
        {
            _isLogout = true;
            if (_zk != null)
                _zk.Dispose();
        }

        /// <summary>
        /// 创建父结点
        /// </summary>
        private void CheckNodeParent()
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
                            _zk.Create(zNode, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                        else
                            _zk.Create(zNode, null, _zk_Acl, CreateMode.Persistent);
                    }
                }
                catch (Exception ex)
                {
                    ThriftLog.Error("zk:创建父结点异常:" + ex.Message + ex.StackTrace);
                }
            }
        }

        /// <summary>
        ///  注册服务直到成功
        ///  具体RPC服务的注册路径为: /rpc/{namespace}/{service}/{version}, 该路径上的节点都是永久节点
        ///  RPC服务集群节点的注册路径为: /rpc/{namespace}/{service }/{version}/ip:port:weight, 末尾的节点是临时节点.
        /// </summary>
        private void Regedit(string zNode)
        {
            int existsCount = 0; //已经存在次数
            int errorCount = 0; //错误次数

            bool isRegister = false;
            while (!isRegister)
            {
                if (_isLogout) break;

                try
                {
                    _zk.Create(zNode, null, _zk_Acl, CreateMode.Ephemeral);
                    isRegister = true;
                    ThriftLog.Info($"{zNode}节点注册完成 ");
                }
                catch (KeeperException.NodeExistsException ex)
                {
                    existsCount++;
                    errorCount++;
                    ThriftLog.Info($"{zNode}节点已经存在 ");
                }
                catch (Exception ex)
                {
                    errorCount++;
                    ThriftLog.Info(zNode + ex.Message);
                }

                System.Threading.Thread.Sleep(10000);

                if (existsCount > 10) break;
                if (errorCount > 20) break;
            }
        }


        public void Process(WatchedEvent @event)
        {
            if (@event.State == KeeperState.Disconnected)
            {
                if (_isLogout) return;
                ThriftLog.Info("WatchedEvent :" + @event.State.ToString() + " 重新注册zk");
                Start();
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
