﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace Trik.Upload_Extension
{
    internal class StatusbarImpl : IDisposable
    {
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly IVsStatusbar _statusbar;
        private uint _statusbarCookie;
        private BackgroundWorker _worker;
        public bool InProgress { get; private set; }

        internal StatusbarImpl(IVsStatusbar statusbar)
        {
            _statusbar = statusbar;
        }

        public void Dispose()
        {
            _resetEvent.Dispose();
            _worker.Dispose();
        }

        internal void SetText(string text)
        {
            _statusbar.SetText(text);
        }

        internal void Progress(int period, string text)
        {
            if (InProgress) return;
            InProgress = true;
            _resetEvent.Reset();
            _worker = new BackgroundWorker {WorkerSupportsCancellation = true};
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
                        if (worker.CancellationPending) break;
                        _statusbar.Progress(ref _statusbarCookie, 1, text + " " + messageTail, i, iterations);
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
            if (!InProgress) return true;
            _worker.RunWorkerCompleted += (sender, args) => { _statusbar.Progress(ref _statusbarCookie, 1, "", 0, 0);
                                                              InProgress = false; };
            _worker.CancelAsync();
            return await Task.Run(() => _resetEvent.WaitOne());
        }
    }
}