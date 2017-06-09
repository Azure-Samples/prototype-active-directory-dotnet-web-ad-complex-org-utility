using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Events
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string ConnectionId { get; set; }
        public string ApiKey { get; set; }
    }

    public class PingEvent
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; }

        public PingEvent(string message, DateTime startTime, DateTime endTime)
        {
            Message = message;
            StartTime = startTime;
            EndTime = endTime;
        }
        public PingEvent(string message, DateTime endTime)
        {
            Message = message;
            EndTime = endTime;
        }
        public PingEvent(DateTime startTime)
        {
            StartTime = startTime;
        }
    }

    public class StatusEvent
    {
        public string Message { get; set; }

        public StatusEvent(string message)
        {
            Message = message;
        }
        public StatusEvent(string message, params object[] args)
        {
            Message = string.Format(message, args);
        }
    }

    public class ErrorEvent
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public Exception Exception { get; set; }
        public EventLogEntryType LogEntryType { get; set; }
        public int EventId { get; set; }

        public ErrorEvent()
        {

        }
        public ErrorEvent(string message)
        {
            Message = message;
        }
        public ErrorEvent(string message, Exception ex)
        {
            Message = message;
            Exception = ex;
        }
        public ErrorEvent(string message, EventLogEntryType entryType, Exception ex)
        {
            Message = message;
            LogEntryType = entryType;
            Exception = ex;
        }
        public ErrorEvent(string message, EventLogEntryType entryType, int eventId, Exception ex)
        {
            Message = message;
            Exception = ex;
            LogEntryType = entryType;
            EventId = eventId;
        }
    }
}

