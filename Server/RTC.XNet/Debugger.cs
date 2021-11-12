using System;
using System.Diagnostics;

namespace RTC.XNet
{

    /// <summary>
    /// author:xxp
    /// </summary>
    public static class Debugger
    {
        public static LoggerType LoggerLevel = LoggerType.Debug;
        
        public static void Log(object msg)
        {
            DoLog(LoggerType.Log, msg.ToString());
        }

        public static void LogWaring(object msg)
        {
            DoLog(LoggerType.Waring, msg.ToString());
        }

        public static void LogError(object msg)
        {
            DoLog(LoggerType.Error, msg.ToString());
        }

        public static void DebugLog(string msg)
        {
            DoLog(LoggerType.Debug, msg);
        }

        public static void LogIfLevel(LoggerType level, Func<string> action)
        {
            if (level < LoggerLevel) return;
            var log = action?.Invoke();
            if (log == null) return;
            var dLog = new DebuggerLog()
            {
                LogTime = DateTime.Now,
                Type = level,
                Message = log
            };
            Logger?.Invoke(dLog);
        }

        private static void DoLog(LoggerType type, string msg)
        {
            var log = new DebuggerLog()
            {
                LogTime = DateTime.Now,
                Message = msg,
                Type = type
            };
            Logger?.Invoke(log);
        }

        public static LogWriter Logger { set; get; }

        public static void SetConsole()
        {
            Logger = log => Console.WriteLine(log.ToString());
        }
    }

    /// <summary>
    /// 日志记录者
    /// </summary>
    public delegate void LogWriter(DebuggerLog log);


    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LoggerType
    {
        Debug = 0,
        Log = 1,
        Waring = 2,
        Error = 3
    }

    /// <summary>
    /// 日志
    /// </summary>
    public class DebuggerLog
    {
        public LoggerType Type { set; get; }
        public string Message { set; get; }
        public DateTime LogTime { set; get; }

        public override string ToString()
        {
            return $"[{Type}][{LogTime}]:{Message}";
        }
    }
    


}