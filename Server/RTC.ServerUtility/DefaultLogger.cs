using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Grpc.Core.Logging;
using RTC.XNet;
using Debugger = RTC.XNet.Debugger;

namespace RTC.ServerUtility
{
    public class DefaultLogger : ILogger, IDisposable
    {
        public DefaultLogger()
        {
            //Producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void Debug(string message)
        {
            Debugger.DebugLog(message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            Debugger.DebugLog(string.Format(format, formatArgs));
        }

        public void Dispose()
        {
           
        }

        public void Error(string message)
        {
            Debugger.LogError(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            Debugger.LogError(string.Format(format, formatArgs));
        }

        public void Error(Exception exception, string message)
        {
            Debugger.LogError(exception.ToString());
            Debugger.LogError(message);
        }

        public ILogger ForType<T>()
        {
            return this;
        }

        public void Info(string message)
        {
            Debugger.Log(message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            Debugger.Log(string.Format(format, formatArgs));
        }

        public void Warning(string message)
        {
            Debugger.LogWaring(message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            Debugger.LogWaring(string.Format(format, formatArgs));
        }

        public void Warning(Exception exception, string message)
        {
            Debugger.LogWaring(exception.ToString());
            Debugger.LogWaring(message);
        }

        public  void WriteLog(DebuggerLog log)
        {
            switch (log.Type)
            {
                case LoggerType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LoggerType.Waring:
                case LoggerType.Debug:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }
            
            
            
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}->{log}");
            Console.ResetColor();

            if (log.Type 
                is LoggerType.Error
                or LoggerType.Debug 
                or LoggerType.Waring)
            {
                var info = new StackTrace();
                Console.WriteLine(info.ToString());
            }
        }
    }
}