using System;
using System.Diagnostics;

namespace CtrlWiz.Logging.Utility
{
    internal static class ExtensionMethods
    {
        public static string GetExceptionInfo(this Exception ex)
        {
            StackTrace stackTrace = new StackTrace(ex, true);
            string className = stackTrace.GetFrame(0).GetMethod().ReflectedType.FullName;
            string methodName = stackTrace.GetFrame(0).GetMethod().Name;
            string line = stackTrace.GetFrame(0).GetFileLineNumber().ToString();
            string column = stackTrace.GetFrame(0).GetFileColumnNumber().ToString();

            return $"{className}:{methodName}() at line({line}), column({column}).\n{stackTrace.ToString()}";
        }
    }
}