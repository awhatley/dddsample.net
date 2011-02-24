using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Infrastructure.Persistence.InMemory;

using NUnit.Framework;

namespace DomainDrivenDelivery.Infrastructure.Tests.Persistence.InMemory
{
    [TestFixture]
    public class TrackingIdGeneratorTest
    {
        private readonly TrackingIdFactory _trackingIdFactory = new TrackingIdFactoryInMem();

        [Test]
        public void testNextTrackingId()
        {
            var trackingId = _trackingIdFactory.nextTrackingId();
            Assert.NotNull(trackingId);

            var trackingId2 = _trackingIdFactory.nextTrackingId();
            Assert.NotNull(trackingId2);
            Assert.False(trackingId.Equals(trackingId2));
        }
    }
}