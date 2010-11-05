using System;

using DomainDrivenDelivery.Domain.Model.Locations;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Model.Locations
{
    [TestFixture]
    public class LocationTest
    {
        private static readonly TimeZoneInfo CET = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        [Test]
        public void testEquals()
        {
            // Same UN locode - equal
            Assert.True(
                new Location(new UnLocode("ATEST"), "test-name", CET, CustomsZone.None).Equals(
                new Location(new UnLocode("ATEST"), "test-name", CET, CustomsZone.None)));

            // Different UN locodes - not equal
            Assert.False(
                new Location(new UnLocode("ATEST"), "test-name", CET, CustomsZone.None).Equals(
                new Location(new UnLocode("TESTB"), "test-name", CET, CustomsZone.None)));

            // Always equal to itself
            Location location = new Location(new UnLocode("ATEST"), "test-name", CET, CustomsZone.None);
            Assert.True(location.Equals(location));

            // Never equal to null
            Assert.False(location.Equals(null));

            // Special NONE location is equal to itself
            Assert.True(Location.None.Equals(Location.None));

            try
            {
                new Location(null, null, null, null);
                Assert.Fail("Should not allow any null constructor arguments");
            }
            catch(ArgumentException)
            {
            }
        }
    }
}