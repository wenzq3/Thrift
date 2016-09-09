using Org.Apache.Zookeeper.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooKeeperNet;

namespace Thrift.Server
{
    public class ZookeeperHelp
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="sessionTimeout">豪秒</param>
        /// <param name="watch"></param>
        /// <returns></returns>
        public static ZooKeeper CreateClient(string host, int sessionTimeout, IWatcher watch, string digest)
        {
            ZooKeeper zk = new ZooKeeper(host, new TimeSpan(0, 0, 0, 0, sessionTimeout), watch);
            if (!string.IsNullOrEmpty(digest))
                zk.AddAuthInfo("digest", digest.GetBytes());
            zk.AddAuthInfo("digest", "guest:guest".GetBytes());

            int count = 0;
            while (zk.State != ZooKeeper.States.CONNECTED)
            {
                if (count++ > 2000)
                {
                    //  throw new Exception("连接异常");
                    return null;
                }
                System.Threading.Thread.Sleep(1);
            }
            return zk;
        }

        private static string generateDigest(string idPassword)
        {
            string[] parts = idPassword.Split(new char[] { ':' });
            string md5 = HashCode(idPassword);
            return parts[0] + ":" + md5;
        }
        private static string HashCode(string str)
        {
            string rethash = "";
            try
            {

                System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
                System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                byte[] combined = encoder.GetBytes(str);
                hash.ComputeHash(combined);
                rethash = Convert.ToBase64String(hash.Hash);
            }
            catch (Exception ex)
            {
                string strerr = "Error in HashCode : " + ex.Message;
            }
            return rethash;
        }

        public static List<ACL> GetACL(string digest)
        {
            List<ACL> acl = new List<ACL>();
            string admin = generateDigest(digest);
            acl.Add(new ACL(Perms.ALL, new ZKId("digest", admin)));

            string guest = generateDigest("guest:guest");
            acl.Add(new ACL(Perms.READ, new ZKId("digest", guest)));
            return acl;
        }

        public static byte[] GetData(ZooKeeper zk, string znode)
        {
            try
            {
                var nodeData = zk.GetData(znode, false, null);
                return nodeData;
            }
            catch (Exception ex)
            {
                ThriftLog.Error(ex.Message + ex.StackTrace);
                return null;
            }
        }

        public static List<string> GetChildren(ZooKeeper zk, string znode)
        {
            try
            {
                var nodeData = zk.GetChildren(znode, false);
                if (nodeData != null)
                    return nodeData.ToList();
                else
                    return null;
            }
            catch (Exception ex)
            {
                ThriftLog.Error(ex.Message + ex.StackTrace);
                return null;
            }
        }
    }
}
