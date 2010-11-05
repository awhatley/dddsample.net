using DomainDrivenDelivery.Domain.Model.Locations;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Tests.Model.Locations
{
    [TestFixture]
    public class CustomsZoneTest
    {
        [Test]
        public void testIncludes()
        {
            Assert.True(L.US.Includes(L.DALLAS));
            Assert.False(L.EU.Includes(L.NEWYORK));
        }

        [Test]
        public void testEntryPoint()
        {
            Assert.AreEqual(L.LONGBEACH, L.US.EntryPoint(L.SHANGHAI, L.LONGBEACH, L.CHICAGO));
        }

        [Test]
        public void testClearancePoint()
        {
            Assert.AreEqual(L.LONGBEACH, L.US.EntryPoint(L.SHANGHAI, L.LONGBEACH, L.CHICAGO));
        }
    }
}