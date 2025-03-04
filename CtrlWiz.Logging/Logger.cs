using CtrlWiz.Logging.Utility;
using System;
using System.IO;
using System.Text;

namespace CtrlWiz.Logging
{
    public static class Logger
    {
        private static string _logFilePath = null;

        public static void SetUpLogger(LoggedApp loggedApp)
        {
            if (_logFilePath == null)
            {
                string logFileName = "CtrlWiz.log";
                if (loggedApp == LoggedApp.NW)
                    logFileName = "CtrlWizNW.log";
                else if (loggedApp == LoggedApp.RVT)
                    logFileName = "CtrlWizRVT.log";

                if (loggedApp == LoggedApp.RVT)
                    _logFilePath = Path.Combine(Path.GetTempPath(), "..", logFileName);
                else
                    _logFilePath = Path.Combine(Path.GetTempPath(), logFileName);
            }
        }

        private static string CreateExceptionMessage(Exception ex)
        {
            return $"[{DateTime.UtcNow} UTC+0] {ex.Message} {ex.GetExceptionInfo()}";
        }
        public static void LogException(this Exception ex)
        {
            File.AppendAllText(_logFilePath, CreateExceptionMessage(ex), Encoding.UTF8);

            if (ex.InnerException != null)
            {
                File.AppendAllText(_logFilePath, CreateExceptionMessage(ex.InnerException), Encoding.UTF8);
            }
        }
    }
}