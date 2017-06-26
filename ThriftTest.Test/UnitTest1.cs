using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rabbit.Zookeeper.Implementation;
using System.Text;

namespace Rabbit.Zookeeper
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {

            IZookeeperClient client = new ZookeeperClient(new ZookeeperClientOptions("192.168.1.181:2182,192.168.1.182:2182,192.168.1.183:2182")
            {
                BasePath = "/", //default value
                ConnectionTimeout = TimeSpan.FromSeconds(10), //default value
                SessionTimeout = TimeSpan.FromSeconds(20), //default value
                OperatingTimeout = TimeSpan.FromSeconds(60), //default value
                ReadOnly = false, //default value
                SessionId = 0, //default value
                SessionPasswd = null ,//default value
            EnableEphemeralNodeRestore = true //default value
            });


            var data = Encoding.UTF8.GetBytes("2016");

            //Fast create temporary nodes
            var val= client.CreateEphemeralAsync("/wzq", data);

          //  while(true)

        }
    }
}
