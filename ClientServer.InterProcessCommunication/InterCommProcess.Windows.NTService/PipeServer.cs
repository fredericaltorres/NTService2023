using System;
using System.Threading;
using System.IO;
using PlatformStandardToPlatformBridgeLib;
using Newtonsoft.Json;

namespace InterCommProcess.Windows.NTService
{
    public class PipeServer
    {
        public static void Trace(string s)
        {
            NTService.Trace(s);
        }

        static Thread[] servers = new Thread[1];
        private static int numThreads = 1;
        public static void Run(int counter)
        {
            Trace($"{Environment.NewLine}Listening... count:{counter++}. Press CTRL+C to terminate the process");
            BridgeServer.RunOnce(ServerThread);
        }

        private static void ServerThread(object data)
        {
            var bridgeServer = new BridgeServer();
            bridgeServer.WaitForConnection();
            try
            {
                var cmdStr = bridgeServer.ReadString();
                Trace($"[{bridgeServer.ThreadId}, {bridgeServer.GetImpersonationUserName()}]{cmdStr}");
                var command = bridgeServer.ParseCommand(cmdStr);
                var answer = command.ExecuteCommand();
                var json = JsonConvert.SerializeObject(answer, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Newtonsoft.Json.Formatting.Indented });
                Trace(json);
                bridgeServer.WriteString(json);

                //bridgeServer._pipeServer.RunAsClient(fileReader.Start);
            }
            catch (IOException e)
            {
                Trace($"ERROR: {e}");
            }
            finally
            {
                bridgeServer.Close();
            }
        }
    }
}
