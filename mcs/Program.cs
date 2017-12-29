using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using NLog.Extensions.Logging;
using NLog.Config;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

namespace mcs
{
    class Program
    {
        class CombineConsoleLogger : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;

            private StreamWriter _standardOuput = new StreamWriter(Console.OpenStandardOutput());
            public CombineConsoleLogger()
            {
            }

            public override void Write(string value)
            {
                _standardOuput.Write(value);
                Logger.LogInformation(value);
            }

            public override void WriteLine(string value)
            {
                _standardOuput.WriteLine(value);
                Logger.LogInformation(value);
            }

            protected override void Dispose(bool disposing)
            {
                _standardOuput.Flush();
            }
        }

        public static IConfiguration Configuration { get; private set; }
        public static IServiceProvider ServiceProvider { get; private set; }

        public static ILogger<Program> Logger { get; private set; }

        static string WorkDirectory = Directory.GetCurrentDirectory();

        static int Main(string[] args)
        {
            WorkDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            Configuration = LoadConfiguration();
            var serviceColletion = new ServiceCollection();
            ConfigureServices(serviceColletion);
            ServiceProvider = serviceColletion.BuildServiceProvider();
            DoConfigure(ServiceProvider);
            Logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

            using (Logger.BeginScope("mcs"))
            {
                Logger.LogInformation($"WorkDirectory:{WorkDirectory}");
                using (var newOut = new CombineConsoleLogger())
                {
                    Console.SetOut(newOut);

                    return Microsoft.CodeAnalysis.CSharp.CommandLine.Program.Main(PrepareArguments(args));
                }
            }
        }

        private static void DoConfigure(IServiceProvider serviceProvider)
        {
            var method = typeof(Program).GetMethod(nameof(Configure), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var @params = from p in method.GetParameters()
                          select serviceProvider.GetRequiredService(p.ParameterType);
            method.Invoke(null, @params.ToArray());
        }

        private static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(WorkDirectory,"config.json"), false, false);
            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection serviceProvider)
        {
            serviceProvider.AddOptions();
            serviceProvider.AddLogging();
        }

        private static void Configure(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();
            var loggingConfiguration = new LoggingConfiguration();
            loggingConfiguration.AddTarget(new NLog.Targets.FileTarget("file_target") {
                FileName =Path.GetFullPath(@"mcs.log"),
                DeleteOldFileOnStartup = true,
                KeepFileOpen = true,
                AutoFlush = false
            });
            loggingConfiguration.AddRuleForAllLevels("file_target");
            loggerFactory.ConfigureNLog(loggingConfiguration);
        }

        static string[] PrepareArguments(string[] args)
        {
            var arguments = new List<string>();
            //TODO:read from config
            var flags = new string[] {
                "-noconfig",
                "-nostdlib+",
                "-nologo"
            };
            arguments.AddRange(flags);

            //prepare System.*.dll reference
            var response_file = args[0].Substring(1);
            var unity_compiler_options = File.ReadAllLines(response_file);

            var smcs_location = Environment.GetEnvironmentVariable("smcs");
            var sdkline = Array.Find(unity_compiler_options, line => line.Contains("-sdk:"));
            if (sdkline != null && sdkline.Contains("4.6"))
                ;//TODO
            else
            {
                //TODO Platfom dispatch
                //Windows
                var program_dir = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)");
                var profile_partdir = @"Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile";

                //TODO dpendents on smcs location
                var full_dir = "Unity Full v3.5";
                var subset_dir = "Unity Subset v3.5";

                //TODO:read from config
                var reference_dlls = new string[]
                {
                    "mscorlib.dll",
                    "System.dll",
                    "System.Core.dll",
                    "System.Xml.dll"
                };

                arguments.AddRange(
                    reference_dlls
                    .Select(dllname => $"-r:\"{Path.Combine(program_dir,profile_partdir, full_dir,dllname)}\"")
                    );
            }

            arguments.AddRange(args);

            Logger.LogInformation($"Source Location:{smcs_location} Original Arguments:{string.Join(" ", args)} Target Arguments:{string.Join(" ", arguments)}");
            return arguments.ToArray();
        }
    }
}
