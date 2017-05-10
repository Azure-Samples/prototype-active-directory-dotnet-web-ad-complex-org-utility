using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexOrgSTS.Infrastructure
{
    public static class Logging
    {
        public static bool AlertsEnabled { get; set; }
        public static string AlertRecipients { get; set; }
        public static string AppServer { get; set; }

        /// <summary>
        /// Writes an error entry to the Application log, Application Source. This is a fallback error writing mechanism.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="errorType">Type of error.</param>
        /// <param name="ex">Original exception (optional)</param>
        public static void WriteToAppLog(string message, EventLogEntryType errorType, Exception ex = null)
        {
            if (ex != null)
            {
                message += GetFormattedException(ex);
            }
            EventLog.WriteEntry("Application", message, errorType, 0);
        }
        
        private static string GetFormattedException(Exception ex)
        {
            var message = " (original error: " + ex.Source + "/" + ex.Message + "\r\nStack Trace: " +
                ex.StackTrace + ")";
            if (ex.InnerException != null)
            {
                message += "\r\nInner Exception: " + ex.GetBaseException();
            }
            return message;
        }
    }
}
