using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace PlatformStandardToPlatformBridgeLib
{
    public enum Method
    {
         FileExists, DirExists, Undefined, FilesExists, Ping, DirCreate
    }

    public class CommandAnswer
    {
        public List<bool> Result { get; set; }
        public Dictionary<string, bool> DicResult { get; set; }
        public string ErrorMessage { get; set; } = null;
        public bool Succeeded => this.ErrorMessage == null;

        public string ServerName => Environment.MachineName;
        public DateTime ServerTime => DateTime.Now;

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class Command
    {
        public Method Method;
        public List<string> Parameters;

        public string ToJSON()
        {
            var json = JsonConvert.SerializeObject(this);
            return json;
        }

        public static Command Parse(string jsonCommand)
        {
            var c = JsonConvert.DeserializeObject<Command>(jsonCommand);
            return c;
        }

        public CommandAnswer ExecuteCommand()
        {
            var results =  new List<bool>();
            var dicResults = new Dictionary<string, bool>();
            var errorMessage = null as string;
            switch(this.Method)
            {
                case Method.FilesExists:
                {
                    foreach (var file in this.Parameters)
                        dicResults.Add(file, File.Exists(file));
                } 
                break;
                case Method.Ping: results.Add(true); break;
                case Method.FileExists: results.Add(File.Exists(this.Parameters[0])); break;
                case Method.DirExists: results.Add(Directory.Exists(this.Parameters[0])); break;
                case Method.DirCreate:
                    try
                    {
                        Directory.CreateDirectory(this.Parameters[0]);
                        results.Add(true);
                    }
                    catch (Exception ex)
                    {
                        results.Add(false);
                        errorMessage = ex.Message;
                    }
                    break;
                default: errorMessage = $"[ERROR]Unknown command:{this.Method}"; break;
            }
            return new CommandAnswer { Result = results,  DicResult = dicResults, ErrorMessage = errorMessage };
        }
    }

    public class BridgeCommon
    {
        public const string NamePipeName = "InterCommProcess.Service.Lib.{33BC2642-9401-4AD4-B07D-55E7E0453EBA}";
        public const string AckMessage = "Trust me server!";
    }
}
