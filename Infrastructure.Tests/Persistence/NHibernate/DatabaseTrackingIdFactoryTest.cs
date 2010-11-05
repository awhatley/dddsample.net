using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Infrastructure.Persistence.NHibernate;

using NUnit.Framework;

namespace DomainDrivenDelivery.Infrastructure.Tests.Persistence.NHibernate
{
    [TestFixture]
    public class DatabaseTrackingIdFactoryTest : AbstractRepositoryTest
    {
        public DatabaseTrackingIdFactory TrackingIdFactory { get; set; }

        [Test]
        public void testNext()
        {
            TrackingId id1 = TrackingIdFactory.nextTrackingId();
            TrackingId id2 = TrackingIdFactory.nextTrackingId();
            Assert.NotNull(id1);
            Assert.NotNull(id2);
            Assert.False(id1.Equals(id2));
        }
    }
}