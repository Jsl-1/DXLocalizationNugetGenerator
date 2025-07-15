using DXLocalizationNugetGenerator.Command;
using ManyConsole;
using System;
using System.Collections.Generic;

namespace DXLocalizationNugetGenerator
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            args = new[] { nameof(CreateNuget),
                @"-i=D:\Temp\DevExpressLocalizationPackages\nuspec",
                @"-o=D:\Temp\DevExpressLocalizationPackages\nuget",
            };

            //args = new[] { nameof(CreateNuspec),
            //    @"-inputDXNuGetPath=C:\Program Files\DevExpress 25.1\Components\System\Components\packages",
            //    @"-inputLocalizationNetFrameworkPath=D:\Temp\DevExpressLocalizationPackages\sources\Framework",
            //    @"-inputLocalizationNetCorePath=D:\Temp\DevExpressLocalizationPackages\sources\NetCore",
            //    @"-outputLanguageCode=fr",
            //    @"-outputNuspecPath=D:\Temp\DevExpressLocalizationPackages\nuspec",
            //    @"-r=1" //minor-version
            //};
#endif

            // locate any commands in the assembly (or use an IoC container, or whatever source)
            var commands = GetCommands();

            // then run them.
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
