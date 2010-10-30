using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Tests.Model.Handling
{
    [TestFixture]
    public class HandlingEventTest
    {
        private Cargo cargo;

        [SetUp]
        protected void setUp()
        {
            TrackingId trackingId = new TrackingId("XYZ");
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG, L.NEWYORK, DateTime.Now);
            cargo = new Cargo(trackingId, routeSpecification);
        }

        [Test]
        public void testNewWithCarrierMovement()
        {
            HandlingEvent e1 = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.LOAD,
                L.HONGKONG,
                SampleVoyages.continental1,
                new OperatorCode("ABCDE"));
            Assert.AreEqual(L.HONGKONG, e1.location());

            HandlingEvent e2 = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.UNLOAD,
                L.NEWYORK,
                SampleVoyages.continental1,
                new OperatorCode("ABCDE"));
            Assert.AreEqual(L.NEWYORK, e2.location());

            // These event types prohibit a carrier movement association
            foreach(
                HandlingActivityType type in
                    new[] {HandlingActivityType.CLAIM, HandlingActivityType.RECEIVE, HandlingActivityType.CUSTOMS})
            {
                try
                {
                    new HandlingEvent(cargo,
                        DateTime.Now,
                        DateTime.Now,
                        type,
                        L.HONGKONG,
                        SampleVoyages.continental1,
                        new OperatorCode("ABCDE"));
                    Assert.Fail("Handling event type " + type + " prohibits carrier movement");
                }
                catch(ArgumentException expected)
                {
                }
            }

            // These event types requires a carrier movement association
            foreach(HandlingActivityType type in new[] {HandlingActivityType.LOAD, HandlingActivityType.UNLOAD})
            {
                try
                {
                    new HandlingEvent(cargo,
                        DateTime.Now,
                        DateTime.Now,
                        type,
                        L.HONGKONG,
                        null,
                        new OperatorCode("ABCDE"));
                    Assert.Fail("Handling event type " + type + " requires carrier movement");
                }
                catch(ArgumentException expected)
                {
                }
            }
        }

        [Test]
        public void testNewWithLocation()
        {
            HandlingEvent e1 = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.CLAIM,
                L.HELSINKI);
            Assert.AreEqual(L.HELSINKI, e1.location());
        }

        [Test]
        public void testCurrentLocationLoadEvent()
        {
            HandlingEvent ev = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.LOAD,
                L.CHICAGO,
                SampleVoyages.continental2,
                new OperatorCode("ABCDE"));

            Assert.AreEqual(L.CHICAGO, ev.location());
        }

        [Test]
        public void testCurrentLocationUnloadEvent()
        {
            HandlingEvent ev = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.UNLOAD,
                L.HAMBURG,
                SampleVoyages.continental2,
                new OperatorCode("ABCDE"));

            Assert.AreEqual(L.HAMBURG, ev.location());
        }

        [Test]
        public void testCurrentLocationReceivedEvent()
        {
            HandlingEvent ev = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.RECEIVE,
                L.CHICAGO);

            Assert.AreEqual(L.CHICAGO, ev.location());
        }

        [Test]
        public void testCurrentLocationClaimedEvent()
        {
            HandlingEvent ev = new HandlingEvent(cargo,
                DateTime.Now,
                DateTime.Now,
                HandlingActivityType.CLAIM,
                L.CHICAGO);

            Assert.AreEqual(L.CHICAGO, ev.location());
        }

        [Test]
        public void testParseType()
        {
            Assert.AreEqual(HandlingActivityType.CLAIM, Enum.Parse(typeof(HandlingActivityType), "CLAIM"));
            Assert.AreEqual(HandlingActivityType.LOAD, Enum.Parse(typeof(HandlingActivityType), "LOAD"));
            Assert.AreEqual(HandlingActivityType.UNLOAD, Enum.Parse(typeof(HandlingActivityType), "UNLOAD"));
            Assert.AreEqual(HandlingActivityType.RECEIVE, Enum.Parse(typeof(HandlingActivityType), "RECEIVE"));
        }

        [Test]
        public void testParseTypeIllegal()
        {
            try
            {
                Enum.Parse(typeof(HandlingActivityType), ("NOT_A_HANDLING_EVENT_TYPE"));
                Assert.Fail("Expected IllegaArgumentException to be thrown");
            }
            catch(ArgumentException e)
            {
                // All's well
            }
        }

        [Test]
        public void testEqualsAndSameAs()
        {
            DateTime timeOccured = DateTime.Now;
            DateTime timeRegistered = DateTime.Now;

            HandlingEvent ev1 = new HandlingEvent(cargo,
                timeOccured,
                timeRegistered,
                HandlingActivityType.LOAD,
                L.CHICAGO,
                SampleVoyages.atlantic1,
                new OperatorCode("ABCDE"));
            HandlingEvent ev2 = new HandlingEvent(cargo,
                timeOccured,
                timeRegistered,
                HandlingActivityType.LOAD,
                L.CHICAGO,
                SampleVoyages.atlantic1,
                new OperatorCode("ABCDE"));

            Assert.IsTrue(ev1.Equals(ev2));
            Assert.IsTrue(ev2.Equals(ev1));

            Assert.IsTrue(ev1.Equals(ev1));

            //noinspection ObjectEqualsNull
            Assert.IsFalse(ev2.Equals(null));

            Assert.IsFalse(ev2.Equals(new object()));
        }
    }
}