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
    public class HandlingHistoryTest
    {
        private Cargo cargo;
        private Cargo cargo2;
        private Voyage voyage;
        private HandlingEvent event1;
        private HandlingEvent event1duplicate;
        private HandlingEvent event2;
        private HandlingEvent eventOfCargo2;
        private HandlingHistory handlingHistory;

        [SetUp]
        protected void setUp()
        {
            cargo = new Cargo(new TrackingId("ABC"),
                new RouteSpecification(L.SHANGHAI, L.DALLAS, DateTime.Parse("2009-04-01")));
            cargo2 = new Cargo(new TrackingId("DEF"),
                new RouteSpecification(L.SHANGHAI, L.NEWYORK, DateTime.Parse("2009-04-15")));

            voyage =
                new Voyage.Builder(new VoyageNumber("X25"), L.HONGKONG).addMovement(L.SHANGHAI,
                    new DateTime(1),
                    new DateTime(2)).addMovement(L.DALLAS, new DateTime(3), new DateTime(4)).build();
            event1 = new HandlingEvent(cargo,
                DateTime.Parse("2009-03-05"),
                DateTime.Parse("2009-03-05"),
                HandlingActivityType.LOAD,
                L.SHANGHAI,
                voyage,
                new OperatorCode("ABCDE"));
            event1duplicate = new HandlingEvent(cargo,
                DateTime.Parse("2009-03-05"),
                DateTime.Parse("2009-03-07"),
                HandlingActivityType.LOAD,
                L.SHANGHAI,
                voyage,
                new OperatorCode("ABCDE"));
            event2 = new HandlingEvent(cargo,
                DateTime.Parse("2009-03-10"),
                DateTime.Parse("2009-03-06"),
                HandlingActivityType.UNLOAD,
                L.DALLAS,
                voyage,
                new OperatorCode("ABCDE"));
            eventOfCargo2 = new HandlingEvent(cargo2,
                DateTime.Parse("2009-03-11"),
                DateTime.Parse("2009-03-08"),
                HandlingActivityType.LOAD,
                L.GOTHENBURG,
                voyage,
                new OperatorCode("ABCDE"));
        }

        [Test]
        public void testDistinctEventsByCompletionTime()
        {
            var hashCodeActivity1 = event2.activity().GetHashCode();
            var hashCodeActivity2 = event1.activity().GetHashCode();
            var hashCodeActivity3 = event1duplicate.activity().GetHashCode();

            handlingHistory = HandlingHistory.fromEvents(new[] {event2, event1, event1duplicate});

            Assert.AreEqual(new[] {event1, event2}, handlingHistory.distinctEventsByCompletionTime());
        }

        [Test]
        public void testMostRecentlyCompletedEvent()
        {
            handlingHistory = HandlingHistory.fromEvents(new[] {event2, event1, event1duplicate});

            Assert.AreEqual(event2, handlingHistory.mostRecentlyCompletedEvent());
        }

        [Test]
        public void testMostRecentLoadOrUnload()
        {
            // TODO
            HandlingEvent event3Customs = new HandlingEvent(cargo,
                DateTime.Parse("2009-03-11"),
                DateTime.Parse("2009-03-11"),
                HandlingActivityType.CUSTOMS,
                L.DALLAS);
            handlingHistory = HandlingHistory.fromEvents(new[] {event2, event1, event1duplicate, event3Customs});

            Assert.AreEqual(event3Customs, handlingHistory.mostRecentlyCompletedEvent());
            Assert.AreEqual(event2, handlingHistory.mostRecentPhysicalHandling());
        }

        [Test]
        public void testUniqueCargoOfEvents()
        {
            try
            {
                handlingHistory = HandlingHistory.fromEvents(new[] {event1, event2, eventOfCargo2});
                Assert.Fail("A handling history should only accept handling events for a single unique cargo");
            }
            catch(ArgumentException expected)
            {
            }
        }

        [Test]
        public void testCargo()
        {
            handlingHistory = HandlingHistory.fromEvents(new[] {event1, event2});
            Assert.AreEqual(cargo, handlingHistory.cargo());

            handlingHistory = HandlingHistory.fromEvents(new[] {eventOfCargo2});
            Assert.AreEqual(cargo2, handlingHistory.cargo());
        }
    }
}