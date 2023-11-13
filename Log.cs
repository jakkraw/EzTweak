using System;
using System.IO;

namespace EzTweak {
    public static class Log {
        public static string log_file = "eztweak.log";

        public static Action<string> pipe = (text) => {
            Console.Out.Write(text);

        };
        public static void Write(string text) {
            pipe(text);
            try {
                File.AppendAllText(log_file, text);
            } catch (Exception e) {
                e.ToString();
            }

        }

        public static void WriteLine(string text) {
            var level = "info";
            Write($"{DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss")} {level}: {text}{Environment.NewLine}");
        }
    }

    public static class Status
    {
        public static Action<string> pipe = (text) => {
            Console.Out.Write(text);
        };
        public static void Write(string text)
        {
            pipe(text);
        }

        public static void Update(string text)
        {
            Write($"{text}");
        }

        public static void start(string name, string action)
        {
            status($"⌛ {name}", action);
        }

        public static void done(string name, string action)
        {
            status($"✅ {name}", action);
        }

        static void status(string name, string action)
        {
            var msg = $"{name} ➤ {action}";
            Log.WriteLine(msg);
            Write(msg);
        }
    }
}
