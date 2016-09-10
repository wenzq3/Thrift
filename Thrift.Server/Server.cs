using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Server;
using Thrift.Server.Config;
using Thrift.Transport;

namespace Thrift.Server
{
    public class Server
    {
        private static List<TServer> _services;
        private const int _delayedTime = 20000;//延时关闭服务时间

        public static void Start()
        {
            _services = new List<TServer>();

            var _configPath = ConfigurationManager.AppSettings["ThriftServerConfigPath"];
            Config.ThriftConfigSection config = null;
            if (string.IsNullOrEmpty(_configPath))
                config = ConfigurationManager.GetSection("thriftServer") as Config.ThriftConfigSection;
            else
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                {
                    ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configPath)
                }, ConfigurationUserLevel.None).GetSection("thriftServer") as Config.ThriftConfigSection;
            }

            foreach (Service service in config.Services)
            {
                new System.Threading.Thread(() =>
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, service.HandlerType.Split(',')[1]));
                        object handle = assembly.CreateInstance(service.HandlerType.Split(',')[0], true);

                        if (handle == null)
                            throw new Exception(service.HandlerType + "为空");

                        var processor = (Thrift.TProcessor)Type.GetType(service.ProcessType, true)
                       .GetConstructor(new Type[] { Type.GetType(service.IfaceType, true) })
                          .Invoke(new object[] { handle });

                        TServerTransport serverTransport = new TServerSocket(service.Port, service.ClientTimeout);

                        TServer server = new TThreadPoolServer(new BaseProcessor(processor), serverTransport,
                            new TTransportFactory(),
                            new TTransportFactory(),
                            new TBinaryProtocol.Factory(),
                            new TBinaryProtocol.Factory(), service.MinThreadPoolThreads, service.MaxThreadPoolThreads, (x) =>
                            {
                                ThriftLog.Info("log:" + x);

                            });

                        if (!string.IsNullOrEmpty(service.ZnodeParent))
                            ConfigCenter.RegeditServer(service); //zookeeper 注册服务

                        ThriftLog.Info($"{service.Name} {service.Port} Starting the TThreadPoolServer...");
                        _services.Add(server);
                        server.Serve();
                    }
                    catch (Exception ex)
                    {
                        ThriftLog.Error(ex.Message + ex.StackTrace);
                    }
                }).Start();
            }

        }

        public static void Stop()
        {
            ConfigCenter.LogoutServer(); //先注销zookeeper

            System.Threading.Thread.Sleep(_delayedTime); //延时关闭

            foreach (TServer server in _services)
            {
                if (server != null)
                    server.Stop();
            }
        }
    }
}
