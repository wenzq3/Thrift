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
    public class RegeditConfig
    {
        private ZooKeeper _zk = null;
        private List<ACL> _zk_Acl;
        private Service _service;
        private string _host;
        private string _digest;
        private int _sessionTimeout;
        private bool _isLogout;


        public RegeditConfig(string host, int sessionTimeout, string digest, Service service)
        {
            _host = host;
            _digest = digest;
            _sessionTimeout = sessionTimeout;

            _zk_Acl = ZookeeperHelp.GetACL(_digest);

            _service = service;
            _isLogout = false;
        }

        public ZooKeeper GetZk()
        {
            if (_zk == null)
                _zk = ZookeeperHelp.CreateClient(_host, _sessionTimeout, null, _digest);
            return _zk;
        }

        public void Init()
        {
            string _host = _service.Host;
            if (string.IsNullOrEmpty(_host))
            {
                _host = GetAddressIP();
                if (_host == "")
                    throw new Exception("当前服务IP地址不能为空");
            }
            var zNode = $"{_service.ZnodeParent}/{_host}:{_service.Port}-{_service.Weight}";

            new System.Threading.Thread(() =>
            {
                CheckNodeParent();
                Regedit(zNode);
            }).Start();
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
            string[] list = _service.ZnodeParent.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < list.Count(); i++)
            {
                string zNode = "";
                for (int j = 0; j <= i; j++)
                    zNode += "/" + list[j];

                try
                {
                    var stat = GetZk().Exists(zNode, true);
                    if (stat == null)
                    {
                        if (zNode == "/ThriftServer") //thrift服务的根目录
                            GetZk().Create(zNode, null, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                        else
                            GetZk().Create(zNode, null, _zk_Acl, CreateMode.Persistent);
                    }
                }
                catch (Exception ex)
                {
                    ThriftLog.Error(ex.Message + ex.StackTrace);
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
                    GetZk().Create(zNode, null, _zk_Acl, CreateMode.Ephemeral);
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

        /// <summary>
        /// 获取本地IP地址信息
        /// </summary>
        private string GetAddressIP()
        {
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    return _IPAddress.ToString();
            }

            return "";
        }
    }
}
