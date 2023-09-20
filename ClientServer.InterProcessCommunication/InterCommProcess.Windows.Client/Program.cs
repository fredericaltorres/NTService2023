using PlatformStandardToPlatformBridgeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterCommProcess.Windows.Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("InterCommProcess.Windows.Client");
            var ca = BridgeClient.RunCmd(Method.FileExists, @"C:\Windows\System32\kernel32.dll");
            Console.WriteLine(ca.ToJSON());
        }
    }
}
