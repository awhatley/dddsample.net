using System.Threading;

using DomainDrivenDelivery.Domain.Model.Freight;

namespace DomainDrivenDelivery.Infrastructure.Persistence.InMemory
{
    public class TrackingIdFactoryInMem : TrackingIdFactory
    {
        private static long SEQ;

        public TrackingId nextTrackingId()
        {
            return new TrackingId(Interlocked.Increment(ref SEQ));
        }
    }
}