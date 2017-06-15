using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Server
{
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Messaging;
    using System.Diagnostics;    //RealProxy


    public class MyRealProxy : RealProxy
    {
        private object _target;

        public MyRealProxy(object target) : base(target.GetType())
        {
            this._target = target;
        }
        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
            var url = callMessage.MethodName;
            var args = callMessage.Args;

            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                object returnValue = callMessage.MethodBase.Invoke(this._target, callMessage.Args);

                watch.Stop();
                var time = watch.ElapsedMilliseconds;

                if (Server._funcTime != null)
                    Server._funcTime(url, args, time);

                return new ReturnMessage(returnValue, new object[0], 0, null, callMessage);
            }
            catch (Exception ex)
            {
                if (Server._funcError != null)
                    Server._funcError(url, args,ex);
                throw ex;
            }
        }
    }

    public static class TransparentProxy
    {
        public static object Create(Type t)
        {
            var instance = Activator.CreateInstance(t);

            MyRealProxy realProxy = new MyRealProxy(instance);
            var transparentProxy = realProxy.GetTransparentProxy();
            return transparentProxy;
        }
    }
}
