using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace BrokeredMessageAutoLockRenew
{
    public class BrokeredMessageLockRenewer : IDisposable
    {
        private readonly BrokeredMessage _message;
        private readonly CancellationTokenSource _tokenSource;

        internal BrokeredMessageLockRenewer(BrokeredMessage message, CancellationTokenSource tokenSource)
        {
            _message = message;
            _tokenSource = tokenSource;
            InitializeRenewal();
        }

        void IDisposable.Dispose()
        {
            Cancel();
        }

        private void InitializeRenewal()
        {
            TimeSpan span = (_message.LockedUntilUtc - DateTime.UtcNow);
            TimeSpan delay = TimeSpan.FromTicks((long) (span.Ticks*.9));

            Task.Delay(delay, _tokenSource.Token).ContinueWith(t =>
            {
                if (_tokenSource.IsCancellationRequested) return;

                _message.RenewLock();
                InitializeRenewal();
            }).ContinueWith(t =>
            {
                var exception = t.Exception;

                if (exception != null)
                {
                    exception.Flatten().Handle(e =>
                    {
                        Trace.TraceError(e.Message);
                        return true;
                    });
                }
            }, TaskContinuationOptions.OnlyOnFaulted)
                .ConfigureAwait(false);
        }

        public void Cancel()
        {
            _tokenSource.Cancel(false);
        }
    }
}