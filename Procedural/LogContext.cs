using System;
using System.Collections.Generic;
using System.Linq;
using JiME.Procedural.StoryElements;

namespace JiME.Procedural
{
    public class LogContext
    {
        public List<LogItem> GeneratorLogs { get; private set; } = new List<LogItem>();
        public bool HasErrors { get; private set; }
        
        public void LogInfo(string msg, params object[] args)
        {
            GeneratorLogs.Add(new LogItem()
            {
                Type = LogType.Info,
                Message = string.Format(msg, args)
            });
        }

        public void LogWarning(string msg, params object[] args)
        {
            GeneratorLogs.Add(new LogItem()
            {
                Type = LogType.Warning,
                Message = string.Format(msg, args)
            });
        }

        public void LogError(string msg, params object[] args)
        {
            GeneratorLogs.Add(new LogItem()
            {
                Type = LogType.Error,
                Message = string.Format(msg, args)
            });
            HasErrors = true;
        }

        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        public class LogItem
        {
            public LogType Type { get; set; }
            public string Message { get; set; }
        }
    }
}
