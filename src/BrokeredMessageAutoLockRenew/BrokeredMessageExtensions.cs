using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace BrokeredMessageAutoLockRenew
{
    public static class BrokeredMessageExtensions
    {
        public static BrokeredMessageLockRenewer CreateAutoLockRenewer(this BrokeredMessage message)
        {
            var tokenSource = new CancellationTokenSource();
            return new BrokeredMessageLockRenewer(message, tokenSource);
        }

        public static BrokeredMessageLockRenewer CreateAutoLockRenewer(this BrokeredMessage message, params CancellationToken[] cancellationTokens)
        {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens);
            return new BrokeredMessageLockRenewer(message, tokenSource);
        }
    }
}