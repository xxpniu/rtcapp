using System;

namespace RTC.ServerUtility
{
    /// <summary>
    /// Singleton for server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XSingleton<T> where T : class, new()
    {
        protected XSingleton() { }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class InnerClass
        {
            public static T Inst = new T();
            internal static void Reset()
            {
                Inst = new T();
            }
        }
        
        private static T Singleton => InnerClass.Inst;

        public static T S => Singleton;

        public static void ResetSingle()
        {
            InnerClass.Reset();
        }

    }
}