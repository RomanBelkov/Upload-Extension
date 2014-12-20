﻿using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Trik.Upload_Extension
{
    internal class StatusbarImpl
    {
        private readonly IVsStatusbar _statusbar;
        private uint _statusbarCookie;
        private BackgroundWorker _worker;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);

        internal StatusbarImpl(IVsStatusbar statusbar)
        {
            _statusbar = statusbar;
        }
        internal void SetText(string text)
        {
            _statusbar.SetText(text);
        }

        internal void Progress(int period, string text)
        {
            _resetEvent.Reset();
            _worker = new BackgroundWorker{WorkerSupportsCancellation = true};
            _worker.DoWork += (sender, args) =>
            {
                var worker = sender as BackgroundWorker;
                if (worker == null) return;

                var messageTail = "";
                const int iterations = 10;
                while (!worker.CancellationPending)
                {
                    for (var i = (uint) 0; i < iterations; i++)
                    {
                        _statusbar.Progress(ref _statusbarCookie, 1, text + messageTail, i, iterations);
                        messageTail = "." + ((messageTail.Length < 3) ? messageTail : "");
                        Thread.Sleep(period/iterations);
                    }
                }
                _resetEvent.Set();
            };
            _worker.RunWorkerAsync();
        }

        internal async Task<bool> StopProgressAsync()
        {
            _worker.RunWorkerCompleted += (sender, args) => _statusbar.Progress(ref _statusbarCookie, 1, "", 0, 0);
            _worker.CancelAsync();
            return await Task.Run(() =>_resetEvent.WaitOne());
        }
    }
}
