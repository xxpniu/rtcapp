using System;
using System.Collections.Generic;

namespace RTC.XNet
{
    public delegate bool SubscribeInvoke<in T>(T des);

    public class SubscribeTools<T>: IDisposable
    {
        private readonly List<SubscribeInvoke<T>> _invokes = new List<SubscribeInvoke<T>>();

        private readonly Queue<SubscribeInvoke<T>> _deletes = new Queue<SubscribeInvoke<T>>();

        public void Invoke(T des)
        {
            Debugger.LogIfLevel(LoggerType.Debug, () => $"{des}");
            foreach (var invoke in _invokes)
            {
                if (invoke.Invoke(des))
                    _deletes.Enqueue(invoke);
            }

            while (_deletes.Count > 0)
            {
                _invokes.Remove(_deletes.Dequeue());
            }
        }

        public void Subscribe(SubscribeInvoke<T> process)
        {
            _invokes.Add(process);
        }

        public void SubscribeForever(Action<T> process)
        {
            Subscribe((des) =>
            {
                process?.Invoke(des);
                return false;
            });
        }

        public void SubscribeOnce(Action<T> process)
        {
            Subscribe((des) =>
            {
                process?.Invoke(des);
                return true;
            });
        }

        public void Dispose()
        {
            _invokes.Clear();
        }
        
    }
}