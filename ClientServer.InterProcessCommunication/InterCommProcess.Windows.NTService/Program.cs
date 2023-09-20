using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.HostConfigurators;

namespace InterCommProcess.Windows.NTService
{
    internal static class Program
    {
        // https://topshelf.readthedocs.io/en/latest/configuration/quickstart.html
        internal static class ConfigureService
        {
            internal static void Configure()
            {
                //HostFactory.Run(configure =>
                //{
                //    configure.Service<Service1>(service =>
                //    {
                //        service.ConstructUsing(s => new Service1());
                //        service.WhenStarted(s => s.Start());
                //        service.WhenStopped(s => s.Stop());
                //    });
                //    //Setup Account that window service use to run.  
                //    configure.RunAsLocalSystem();
                //    configure.SetServiceName("MyWindowServiceWithTopshelf");
                //    configure.SetDisplayName("MyWindowServiceWithTopshelf");
                //    configure.SetDescription("My .Net windows service with Topshelf");
                //});
            }
        }

        const string SERVICE_NAME = "InterCommProcess.Windows.NTService";

        /// <summary>
        /// 
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            NTService.Trace(SERVICE_NAME);

            var rc = HostFactory.Run(x =>
            {
                x.UseAssemblyInfoForServiceInfo();

                x.Service(settings => new NTService(), s =>
                {
                    s.BeforeStartingService(_ => NTService.Trace("BeforeStartingService"));
                    s.BeforeStoppingService(_ => NTService.Trace("BeforeStoppingService"));
                });

                x.SetStartTimeout(TimeSpan.FromSeconds(10));
                x.SetStopTimeout(TimeSpan.FromSeconds(10));

                x.SetServiceName(SERVICE_NAME);
                x.SetDisplayName(SERVICE_NAME);
                x.SetDescription(SERVICE_NAME);

                x.OnException((exception) =>
                {
                    Console.WriteLine("Exception thrown - " + exception.Message);
                });
            });
            Console.WriteLine($"rc: {rc}");
        }
    }
}
