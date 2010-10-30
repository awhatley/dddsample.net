using System;

using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Tests.Model.Travel
{
    [TestFixture]
    public class CarrierMovementTest
    {
        [Test]
        public void testConstructor()
        {
            try
            {
                new CarrierMovement(null, null, DateTime.Now, DateTime.Now);
                Assert.Fail("Should not accept null constructor arguments");
            }
            catch(ArgumentException)
            {
            }

            try
            {
                new CarrierMovement(null, null, DateTime.Now, DateTime.Now);
                Assert.Fail("Should not accept null constructor arguments");
            }
            catch(ArgumentException)
            {
            }

            try
            {
                new CarrierMovement(L.STOCKHOLM, null, DateTime.Now, DateTime.Now);
                Assert.Fail("Should not accept null constructor arguments");
            }
            catch(ArgumentException)
            {
            }

            try
            {
                new CarrierMovement(L.STOCKHOLM, L.HAMBURG, new DateTime(200), new DateTime(100));
                Assert.Fail("Should not accept departure time after arrival time");
            }
            catch(ArgumentException)
            {
            }

            try
            {
                new CarrierMovement(L.STOCKHOLM, L.HAMBURG, new DateTime(100), new DateTime(100));
                Assert.Fail("Should not accept arrival time equal to departure time");
            }
            catch(ArgumentException)
            {
            }

            try
            {
                new CarrierMovement(L.STOCKHOLM, L.STOCKHOLM, new DateTime(100), new DateTime(200));
                Assert.Fail("Should not accept identical departure and arrival locations");
            }
            catch(ArgumentException)
            {
            }

            // Ok
            new CarrierMovement(L.STOCKHOLM, L.HAMBURG, new DateTime(100), new DateTime(200));
        }

        [Test]
        public void testSameValueAsEqualsHashCode()
        {
            CarrierMovement cm1 = new CarrierMovement(L.STOCKHOLM, L.HAMBURG, new DateTime(1), new DateTime(2));
            CarrierMovement cm2 = new CarrierMovement(L.STOCKHOLM, L.HAMBURG, new DateTime(1), new DateTime(2));
            CarrierMovement cm3 = new CarrierMovement(L.HAMBURG, L.STOCKHOLM, new DateTime(1), new DateTime(2));
            CarrierMovement cm4 = new CarrierMovement(L.HAMBURG, L.STOCKHOLM, new DateTime(1), new DateTime(2));

            Assert.True(cm1.sameValueAs(cm2));
            Assert.False(cm2.sameValueAs(cm3));
            Assert.True(cm3.sameValueAs(cm4));

            Assert.True(cm1.Equals(cm2));
            Assert.False(cm2.Equals(cm3));
            Assert.True(cm3.Equals(cm4));

            Assert.True(cm1.GetHashCode() == cm2.GetHashCode());
            Assert.False(cm2.GetHashCode() == cm3.GetHashCode());
            Assert.True(cm3.GetHashCode() == cm4.GetHashCode());
        }
    }
}