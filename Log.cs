using System;

namespace EzTweak
{
    public static class Log
    {
        public static Action<string> pipe = (text) => { Console.Out.Write(text); };
        public static void Write(string text)
        {
            pipe(text);
        }

        public static void WriteLine(string text)
        {
            Write(text); Write(Environment.NewLine);
        }
    }
}
