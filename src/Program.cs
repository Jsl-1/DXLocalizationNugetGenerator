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
            // locate any commands in the assembly (or use an IoC container, or whatever source)
            var commands = GetCommands();

#if DEBUG

            var minorVersion = 1;
            var language = "fr";
            var packageIdPrefix = "ApiAndYou";
            var inputDXNuGetPath = @"C:\Program Files\DevExpress 25.2\Components\System\Components\packages";
            var inputLocalizationNetFrameworkPath = @"D:\Temp\DevExpressLocalizationPackages\sources\Framework";
            var inputLocalizationNetCorePath = @"D:\Temp\DevExpressLocalizationPackages\sources\NetCore";          
            var outputNuspecPath = @"D:\Temp\DevExpressLocalizationPackages\nuspec";
            var outputNugetPath = @"D:\Temp\DevExpressLocalizationPackages\nuget";

            foreach(var file in System.IO.Directory.GetFiles(outputNuspecPath))
            {
                System.IO.File.Delete(file);
            }

            foreach (var file in System.IO.Directory.GetFiles(outputNugetPath))
            {
                System.IO.File.Delete(file);
            }


            args = new[] { nameof(CreateNuspec),
                $"-inputDXNuGetPath={inputDXNuGetPath}",
                $"-inputLocalizationNetFrameworkPath={inputLocalizationNetFrameworkPath}",
                $"-inputLocalizationNetCorePath={inputLocalizationNetCorePath}",
                $"-outputLanguageCode={language}",
                $"-outputNuspecPath={outputNuspecPath}",
                $"-r={minorVersion}",
                $"-p={packageIdPrefix}"
            };

            var result = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);

            if (result == 0)
            {
                args = new[] { nameof(CreateNuget),
                    $"-i={outputNuspecPath}",
                    $"-o={outputNugetPath}",
                };

                result = ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }         
            return result;

#else


           

            // then run them.
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
#endif
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
