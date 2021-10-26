#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RTC.ServerUtility
{

    public class App : ServerApp<App>
    {

        private Func<App, Task>? _s, _e;

        public static App Setup(Func<App, Task> setup = default!,
            Func<App, Task> stop = default!)
        {
            var app = S;
            {
                app._s = setup;
                app._e = stop;
            }
            return app;
        }

        protected override async Task Start(CancellationToken? token = default)
        {
            if (_s == null) return;
            await _s.Invoke(this);
        }

        protected override async Task Stop(CancellationToken? token = default)
        {
            if (_e == null) return;
            await _e.Invoke(this);
        }

    }

}