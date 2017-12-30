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
using System.Diagnostics;

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

                    var compiler_ret = Microsoft.CodeAnalysis.CSharp.CommandLine.Program.Main(PrepareArguments(args));
                    if(compiler_ret == 0)
                        pdb2mdb(args);

                    return compiler_ret;
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
            var exist_proj_config = File.Exists("Roslyn.json");
            var builder = new ConfigurationBuilder()
                .SetBasePath(exist_proj_config?Directory.GetCurrentDirectory():WorkDirectory)
                .AddJsonFile(exist_proj_config ? "Roslyn.json":"config.json", false, false);
            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection serviceProvider)
        {
            serviceProvider.AddOptions();
            serviceProvider.AddLogging();
        }

        private static void Configure(ILoggerFactory loggerFactory)
        {
            bool use_log = true;
            bool.TryParse(Configuration.GetSection("Logging")?["OutputLog"], out use_log);
            if (use_log)
            {
                loggerFactory.AddNLog();
                var loggingConfiguration = new LoggingConfiguration();
                loggingConfiguration.AddTarget(new NLog.Targets.FileTarget("file_target")
                {
                    FileName = Path.GetFullPath(@"mcs.log"),
                    DeleteOldFileOnStartup = true,
                    KeepFileOpen = true,
                    AutoFlush = false
                });
                loggingConfiguration.AddRuleForAllLevels("file_target");
                loggerFactory.ConfigureNLog(loggingConfiguration);
            }
        }

        static string[] PrepareArguments(string[] args)
        {
            var roslyn_section = Configuration.GetSection("Roslyn");
            var flags = roslyn_section["Flags"] ?? "-noconfig -nostdlib+ -nologo";
            var arguments = new List<string>();
            arguments.AddRange(flags.Split(' '));

            //prepare System.*.dll reference
            var response_file = args[0].Substring(1);
            var unity_compiler_options = File.ReadAllLines(response_file);

            var sdkline = Array.Find(unity_compiler_options, line => line.Contains("-sdk:"));
            if (sdkline != null && sdkline.Contains("4.6"))
                ;//TODO
            else
            {
                var references = roslyn_section.GetSection("SDK")?["References"] ?? "mscorlib.dll System.dll System.Core.dll System.Xml.dll";
                var reference_dlls = references.Split(' ');

                arguments.AddRange(
                    reference_dlls
                    .Select(dllname => $"-r:\"{Path.Combine(ReferenceDir(roslyn_section), dllname)}\"")
                    );
            }

            ConfigureLangVersion(roslyn_section, arguments);
            arguments.AddRange(args);

            Logger.LogInformation($"Original Arguments:{string.Join(" ", args)} Target Arguments:{string.Join(" ", arguments)}");
            return arguments.ToArray();
        }

        static string ReferenceDir(IConfigurationSection roslyn_section)
        {
            //TODO Platfom dispatch
            //Windows
            var refer_dir = roslyn_section["ReferenceDir"] ?? "%.NetFx%";
            switch (refer_dir)
            {
                case "%.NetFx%":
                    {
                        var program_dir = Environment.GetEnvironmentVariable("PROGRAMFILES(X86)");
                        var profile_partdir = @"Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile";
                        var smcs_location = Environment.GetEnvironmentVariable("smcs").Replace('\\','/');
                        var full_dir = "Unity Full v3.5";
                        var subset_dir = "Unity Subset v3.5";
                        var use_subset = smcs_location.Contains("unity/smcs.exe");
                        return Path.Combine(program_dir, profile_partdir, use_subset ? subset_dir : full_dir);
                    }
            }

            return null;
        }

        static void ConfigureLangVersion(IConfigurationSection roslyn_section,List<string> args)
        {
            var version = roslyn_section["LangVersion"] ?? "5";
            if (version == "Unity")
                version = "5";
            else if (version == "Laster")
                version = "latest";
            else
            {
                int number = 5;
                if (!int.TryParse(version, out number))
                    version = "5";
            }
            args.Add($"/langversion:{version}");
        }

        static void pdb2mdb(string[] args)
        {
            var response_file = args[0].Substring(1);
            var unity_compiler_options = File.ReadAllLines(response_file);
            var targetAssembly = unity_compiler_options.First(line => line.StartsWith("-out:")).Substring(5);

            var process = new Process {
                StartInfo =
                {
                    FileName = Path.Combine(WorkDirectory,"pdb2mdb.exe"),
                    Arguments = targetAssembly,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            Logger.LogInformation($"pdb2mdb {targetAssembly} ExitCode: {process.ExitCode}");
        }
    }
}
