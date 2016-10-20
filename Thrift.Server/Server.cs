using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        private static Dictionary<TServer, RegeditConfig> _services = new Dictionary<TServer, RegeditConfig>();
        private const int _defaultDelayedTime = 20000; //默认延时关闭时间

        public static void Start()
        {
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

                        string assemblyName = service.SpaceName;
                        if (!string.IsNullOrEmpty(service.AssemblyName))
                            assemblyName = service.AssemblyName;

                        var processor = (Thrift.TProcessor)Type.GetType($"{service.SpaceName}.{service.ClassName}+Processor,{assemblyName}", true)
                       .GetConstructor(new Type[] { Type.GetType($"{service.SpaceName}.{service.ClassName}+Iface,{assemblyName}", true) })
                          .Invoke(new object[] { handle });


                        //X509Certificate2 cert = new X509Certificate2("123.pfx", "123");
                        //TServerTransport serverTransport = new TTLSServerSocket(service.Port, service.ClientTimeout, true, cert, null, null);

                        TServerTransport serverTransport = new TServerSocket(service.Port, service.ClientTimeout);

                        TServer server = new TThreadPoolServer(new BaseProcessor(processor, service), serverTransport,
                            new TTransportFactory(),
                            new TTransportFactory(),
                            new TBinaryProtocol.Factory(),
                            new TBinaryProtocol.Factory(), service.MinThreadPoolThreads, service.MaxThreadPoolThreads, (x) =>
                            {
                                ThriftLog.Info("log:" + x);
                            });

                        server.setEventHandler(new ServerEventHandler());

                        RegeditConfig regiditConfig = null;
                        if (service.ZookeeperConfig != null && service.ZookeeperConfig.Host != "")
                            regiditConfig = ConfigCenter.RegeditServer(service); //zookeeper 注册服务

                        ThriftLog.Info($"{service.Name} {service.Port} Starting the TThreadPoolServer...");
                        _services.Add(server, regiditConfig);
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
            //先注销zookeeper
            foreach (var server in _services)
            {
                if (server.Value != null)
                    server.Value.Logout();
            }

            if (_services.Count > 0)
            {
                int delayedTime = _defaultDelayedTime;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ThriftServerStopDelayedTime"]))
                    delayedTime = int.Parse(ConfigurationManager.AppSettings["ThriftServerStopDelayedTime"]);

                System.Threading.Thread.Sleep(delayedTime); //延时关闭
            }

            //再关闭服务
            foreach (var server in _services)
            {
                if (server.Key != null)
                    server.Key.Stop();
            }

            _services.Clear();
        }
    }
}
