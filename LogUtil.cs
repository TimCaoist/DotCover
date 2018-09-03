using System;
using System.IO;
using System.Text;
using System.Threading;

namespace DotCover
{
    public static class LogUtil
    {
        private const string LOGS = "Logs";

        private static string _logFile;
        
        private static readonly StringBuilder Messages = new StringBuilder();

        private static bool isWritting;

        private static int signal;

        private const string DateFormat = "HH:mm:ss";

        private static string LogFile {
            get {
                _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOGS, string.Concat(DateTime.Now.ToString("yyyyMMdd"), ".txt"));
                return _logFile;
            }
        }

        static LogUtil()
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOGS);
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
        }

        public static void Write(string message, string level = "info")
        {
            while (Interlocked.Exchange(ref signal, 1) != 0)//加自旋锁
            {
            }

            var str = string.Concat(DateTime.Now.ToString("HH:mm:ss"), "-", level, "-", message);
            Console.WriteLine(str);
            Messages.AppendLine(str);
            Interlocked.Exchange(ref signal, 0);
            NotifySave();
        }

        public static void Error(Exception ex)
        {
            Write(string.Concat(ex.StackTrace, "@@", ex.Message), "error");
        }

        private static void NotifySave()
        {
            if (isWritting == true)
            {
                return;
            }

            while (Interlocked.Exchange(ref signal, 1) != 0)//加自旋锁
            {
            }

            isWritting = true;
            File.AppendAllText(LogFile, Messages.ToString());
            Messages.Clear();
            isWritting = false;
            Interlocked.Exchange(ref signal, 0);
        }
    }
}
