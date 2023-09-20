using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using static PlatformStandardToPlatformBridgeLib.Command;

namespace PlatformStandardToPlatformBridgeLib
{
    public class BridgeClient
    {
        public NamedPipeClientStream _pipeClient;
        StreamStringServer _communicationStream;

        public BridgeClient()
        {
        }

        public StreamStringServer GetStreamStringServer()
        {
            if (_communicationStream == null)
                _communicationStream = new StreamStringServer(_pipeClient);
            return _communicationStream;
        }


        public const int CONNECTION_TIMEOUT = 400;

        public bool Connect(string server = ".")
        {
            _pipeClient = new NamedPipeClientStream(server, BridgeCommon.NamePipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            _pipeClient.Connect(CONNECTION_TIMEOUT);
            // Console.WriteLine("Connection established");
            this.GetStreamStringServer();
            var r = (this.ReadString() == BridgeCommon.AckMessage);
            if (r)
            {
                // Console.WriteLine("Connection verified");
            }
            else
                Console.WriteLine("Connection verification failed");
            return r;
        }

        public string ReadCommand()
        {
            return this.ReadString();
        }

        public string ReadString()
        {
            return _communicationStream.ReadString();
        }

        public CommandAnswer RunCommand(Method method, List<string> parameters)
        {
            this.WriteCommand(method, parameters);
            var json = this.ReadString();
            return JsonConvert.DeserializeObject<CommandAnswer>(json);
        }
        public CommandAnswer RunCommand(Method method, string parameter)
        {
            return RunCommand(method, new List<string>() { parameter });
        }

        public void WriteCommand(Method method, string parameter)
        {
            WriteCommand(method, new List<string>() { parameter });
        }

        public void WriteCommand(Method method, List<string> parameters)
        {
            var c = new Command() {  Method = method, Parameters = parameters.Select(o => o.ToString()).ToList() };
            this.WriteString(c.ToJSON());
        }

        public void WriteString(string s)
        {
            _communicationStream.WriteString(s);
        }

        public void Close()
        {
            _pipeClient.Close();
        }

        public static CommandAnswer RunCmd(Method method, string parameter, string server = ".")
        {
            return RunCmd(method, new List<string>() { parameter }, server);
        }

        public static CommandAnswer RunCmd(Method method, List<string> parameters, string server = ".")
        {
            BridgeClient bc = null;
            try
            {
                bc = new BridgeClient();
                bc.Connect(server);
                var r = bc.RunCommand(method, parameters);
                bc.Close();
                bc = null;
                Thread.Sleep(50); // Give time to the server to start the next thread
                return r;
            }
            catch(Exception ex)
            {
                return new CommandAnswer { ErrorMessage = ex.Message };
            }
            finally 
            {
                if (bc != null)
                    bc.Close();
            }
        }
    }
}
