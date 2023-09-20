using System;
using System.Threading;
using Topshelf;
using System.IO;
using System.ComponentModel;

namespace Windows.NTService
{
    internal class NTService : ServiceControl
    {
        private bool _mustStop = false;
        private bool _paused = false;

        public NTService()
        {
        }

        public static void Trace(string s)
        {
            Console.WriteLine($"[{DateTime.Now}]{s}");
            File.AppendAllText(@"c:\temp\Simple.Windows.NTService.log", $"[{DateTime.Now}]{s}\r\n");
        }

        public bool Start(HostControl hostControl)
        {
            Trace("Starting...");
            ThreadPool.QueueUserWorkItem(x =>
            {
                var counter = 0;
                while(!_mustStop)
                {
                    if(_paused)
                    {
                        Thread.Sleep(1000); // Support pausing the NT Service
                    }
                    else
                    {
                        DoWork();
                    }
                }
                hostControl.Stop();
            });

            return true;
        }

        int _workCounter = 0;
        private void DoWork()
        {
            Trace($"_workCounter:{_workCounter++}");
            Thread.Sleep(1000*4);
        }

        public bool Stop(HostControl hostControl)
        {
            _mustStop = true;
            Trace("Stopping...");
            return true;
        }

        public bool Pause(HostControl hostControl)
        {
            _paused = true;
            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            _paused = false;
            return true;
        }
    }
}
