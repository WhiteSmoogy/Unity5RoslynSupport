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
            var use_roslyn = Directory.Exists(roslyn_dir);
            string target_exe = Path.Combine(use_roslyn? roslyn_dir:dir, "mcs.exe");
            if (!File.Exists(target_exe))
            {
                Console.WriteLine($"{target_exe} don't exist");
                return 1;
            }
            var compiler_start = new ProcessStartInfo(target_exe, string.Join(" ",args));
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
    }
}
