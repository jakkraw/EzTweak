using System;
using System.IO;

namespace EzTweak
{
    public static class Log
    {
        public static string log_file = "EzTweak.log";

        public static Action<string> pipe = (text) =>
        {
            Console.Out.Write(text);

        };
        public static void Write(string text)
        {
            pipe(text);
            try
            {
                File.AppendAllText(log_file, text);
            }
            catch (Exception e)
            {
                e.ToString();
            }

        }

        public static void WriteLine(string text)
        {
            var level = "info";
            Write($"{DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")} {level}: {text}{Environment.NewLine}");
        }
    }
}
