using System;
using System.Threading;
using Topshelf;
using System.IO;

namespace InterCommProcess.Windows.NTService
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
            File.AppendAllText(@"C:\temp\InterCommProcess.Windows.NTService.log", $"[{DateTime.Now}]{s}\r\n");
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
                        PipeServer.Run(counter++);
                    }
                }
                hostControl.Stop();
            });

            return true;
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
