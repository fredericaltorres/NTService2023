using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace PlatformStandardToPlatformBridgeLib
{
    public class BridgeServer
    {
        public static int NumThreads = 1;
        static Thread[] servers = new Thread[NumThreads];
        public NamedPipeServerStream _pipeServer;
        public int ThreadId;
        public StreamStringServer StreamStringServer;

        public static void Trace(string s)
        {
            Console.WriteLine($"[{DateTime.Now}]{s}");
            File.AppendAllText(@"C:\temp\InterCommProcess.Windows.NTService.log", $"[{DateTime.Now}]{s}\r\n");
        }

        public string GetImpersonationUserName()
        {
            return this._pipeServer.GetImpersonationUserName();
        }

        public void WaitForConnection()
        {
            _pipeServer = new NamedPipeServerStream(BridgeCommon.NamePipeName, PipeDirection.InOut, BridgeServer.NumThreads);
            ThreadId = Thread.CurrentThread.ManagedThreadId;
            
            _pipeServer.WaitForConnection();

            Trace($"Client connected on thread[{ThreadId}].");
            this.StreamStringServer = new StreamStringServer(_pipeServer);
            this.WriteString(BridgeCommon.AckMessage);
        }

        public Command ParseCommand(string cmd)
        {
            return Command.Parse(cmd);
        }

        public string ReadString()
        {
            return StreamStringServer.ReadString();
        }

        public void Close()
        {
            _pipeServer.Close();
        }

        public void WriteString(string s)
        {
            StreamStringServer.WriteString(s);
        }

        public static void RunOnce(ParameterizedThreadStart start)
        {
            CreateNewBackEndServerThreadPools(start);
            WaitForBackEndThreadToBeExausted(NumThreads);
        }

        public static void CreateNewBackEndServerThreadPools(ParameterizedThreadStart start)
        {
            Trace(nameof(CreateNewBackEndServerThreadPools));

            for (var ii = 0; ii < NumThreads; ii++)
            {
                servers[ii] = new Thread(start);
                servers[ii].Start();
            }
            Thread.Sleep(250);
        }

        private static void WaitForBackEndThreadToBeExausted(int threadCount)
        {
            Trace($"{nameof(WaitForBackEndThreadToBeExausted)} threadCount:{threadCount}");
            var threadIndex = threadCount;
            while (threadIndex > 0)
            {
                for (int j = 0; j < threadCount; j++)
                {
                    if (servers[j] != null)
                    {
                        if (servers[j].Join(250))
                        {
                            //Console.WriteLine("Server thread[{0}] finished.", servers[j].ManagedThreadId);
                            servers[j] = null;
                            threadIndex--;    // decrement the thread watch count
                        }
                    }
                }
            }
        }
    }
}
