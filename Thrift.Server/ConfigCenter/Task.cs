using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Thrift.Server
{
    internal sealed class Task
    {
        private readonly Thread _worker;

        private readonly Action _exec;
        private readonly int _interval;
        private readonly int _delay;
        public Task(Action exec, int delay, int interval)
        {
            _exec = exec;
            _delay = delay;
            _interval = interval;

            _worker = new Thread(WorkerMethod);
        }

        public void Start()
        {
            _worker.Start();
        }

        public void Stop()
        {
            _worker.Abort();
        }


        private void WorkerMethod()
        {
            if (_delay > 0)
                Thread.CurrentThread.Join(_delay);

            while (true)
            {
                try
                {
                    _exec();
                    Thread.CurrentThread.Join(_interval);
                }
                catch
                {
                }
            }
        }
    }
}
