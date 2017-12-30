using System;
using System.Diagnostics;
using System.IO;

namespace smcs
{
    class Program
    {
        static int Main(string[] args)
        {
            var location = typeof(Program).Assembly.Location;
            var dir = Path.GetDirectoryName(location);
            var roslyn_dir = Path.Combine(Directory.GetParent(dir).FullName, "Roslyn");
            var use_roslyn = UseRoslyn(roslyn_dir);
            var compiler_start = CompilerDispatch(use_roslyn, roslyn_dir, dir, args);
            if (compiler_start == null)
            {
                Console.WriteLine($"error CS9999: can't find suitbale compiler");
                return 1;
            }
            compiler_start.RedirectStandardError = true;
            compiler_start.RedirectStandardOutput = true;
            compiler_start.UseShellExecute = false;
            compiler_start.EnvironmentVariables["smcs"] = location;

            var compiler_process = new Process();
            compiler_process.StartInfo = compiler_start;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            compiler_process.OutputDataReceived += (sender, e) => Console.Write(e.Data);
            compiler_process.ErrorDataReceived += (sender, e) => Console.Write(e.Data);
            compiler_process.Start();
            compiler_process.BeginOutputReadLine();
            compiler_process.BeginErrorReadLine();
            compiler_process.WaitForExit();

             return compiler_process.ExitCode;
        }

        static ProcessStartInfo CompilerDispatch(bool use_rolsyn,string rolsyn_dir,string assembly_dir,string[] args)
        {
            var mcs_exe = Path.Combine(use_rolsyn? rolsyn_dir:assembly_dir, "mcs.exe");
            if (!File.Exists(mcs_exe))
                return null;
            if (use_rolsyn)
            {
               return new ProcessStartInfo(mcs_exe, string.Join(" ", args));
            }
            var mono_exe = Path.Combine(Directory.GetParent(assembly_dir)//mono
                .Parent//lib
                .Parent//Mono
                .FullName, "bin/mono.exe");
            if (!File.Exists(mono_exe))
                return null;
            return new ProcessStartInfo(mono_exe,$"\"{Path.Combine(assembly_dir,"mcs.exe")}\" {string.Join(" ", args)}");
        }

        static bool UseRoslyn(string roslyn_dir)
        {
            var use_roslyn = Directory.Exists(roslyn_dir);
            if (use_roslyn)
            {
                var config_path = Path.Combine(roslyn_dir, "config.json");
                //优先使用当前目录
                if (File.Exists("Roslyn.json"))
                    config_path = "Roslyn.json";
                try
                {
                    var use_roslyn_line = Array.Find(File.ReadAllLines(config_path), line => line.Contains("UseRoslyn"));
                    if (use_roslyn_line != null)
                    {
                        use_roslyn_line = use_roslyn_line.Trim();
                        var startIndex = use_roslyn_line.IndexOf(':') + 1;
                        var endIndex = use_roslyn_line.IndexOf(',');
                        use_roslyn = bool.Parse(use_roslyn_line.Substring(startIndex, endIndex - startIndex));
                    }
                }
                catch (Exception) { }
            }
            return use_roslyn;
        }
    }
}
