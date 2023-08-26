using System.Diagnostics;

namespace EzTweak
{
    public static class Cmd
    {
        public static string Start(string command, bool quiet = false, string app = "cmd.exe")
        {
            return Start(new[] { command }, quiet, app);
        }

        public static string Start(string[] commands, bool quiet = false, string app = "cmd.exe")
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = $"/C {string.Join("&", commands)}";
            //cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            //foreach (var command in commands)
            //{
            //    cmd.StandardInput.WriteLine(command);
            //    cmd.StandardInput.Flush();
            //}

            //cmd.StandardInput.Close();
            cmd.WaitForExit();
            var output = cmd.StandardOutput.ReadToEnd();
            if (!quiet)
            {
                Log.WriteLine($"{string.Join("&", commands)}");
                Log.WriteLine($"{output}");
            }
            return output;
        }
    }

    public static class Wmic
    {
        public static string Start(string command, bool quiet = false)
        {
            return Start(new[] { command }, quiet);
        }

        public static string Start(string[] commands, bool quiet = false)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "wmic";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();
            foreach (var command in commands)
            {
                cmd.StandardInput.WriteLine(command);
                cmd.StandardInput.Flush();
            }

            cmd.StandardInput.Close();
            cmd.WaitForExit();
            var output = cmd.StandardOutput.ReadToEnd();
            if (!quiet)
            {
                Log.WriteLine($"{output}");
            }
            return output;
        }
    }
}
