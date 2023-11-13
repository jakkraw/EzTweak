using System;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace EzTweak {
    internal static class EzTweak {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.ThreadException += new
            ThreadExceptionEventHandler(HandleUIException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new
            UnhandledExceptionEventHandler(HandleLogicException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new App());
        }

        private static void HandleUIException(object sender, ThreadExceptionEventArgs e)
        {
            var msg = e.Exception?.Message;
            Log.WriteLine($"UI Exception: {msg}");
            MessageBox.Show(msg, "UI Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void HandleLogicException(object sender, UnhandledExceptionEventArgs e)
        {
            var msg = e.ExceptionObject?.ToString();
            Log.WriteLine($"Exception: {msg}");
            MessageBox.Show(msg, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

}