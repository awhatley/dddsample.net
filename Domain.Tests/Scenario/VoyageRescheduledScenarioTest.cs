using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Infrastructure.Persistence.InMemory;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;
using V = DomainDrivenDelivery.Domain.Model.Travel.SampleVoyages;

namespace DomainDrivenDelivery.Domain.Tests.Patterns.Scenario
{
    [TestFixture]
    public class VoyageRescheduledScenarioTest
    {
        private Cargo cargo;
        private Voyage voyage1;
        private Voyage voyage2;
        private Voyage voyage3;

        [SetUp]
        public void setupCargo()
        {
            TrackingIdFactoryInMem trackingIdFactory = new TrackingIdFactoryInMem();

            // Creating new voyages to avoid rescheduling shared ones, breaking other tests
            voyage1 = new Voyage(new VoyageNumber("V1"), V.HONGKONG_TO_NEW_YORK.Schedule);
            voyage2 = new Voyage(new VoyageNumber("V2"), V.NEW_YORK_TO_DALLAS.Schedule);
            voyage3 = new Voyage(new VoyageNumber("V3"), V.DALLAS_TO_HELSINKI.Schedule);

            TrackingId trackingId = trackingIdFactory.nextTrackingId();
            RouteSpecification routeSpecification = new RouteSpecification(L.HANGZOU,
                L.STOCKHOLM,
                DateTime.Parse("2008-12-23"));

            cargo = new Cargo(trackingId, routeSpecification);
            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(voyage1, L.HANGZOU, L.NEWYORK),
                Leg.DeriveLeg(voyage2, L.NEWYORK, L.DALLAS),
                Leg.DeriveLeg(voyage3, L.DALLAS, L.STOCKHOLM));
            cargo.AssignToRoute(itinerary);
        }

        [Test]
        public void voyageIsRescheduledWithMaintainableRoute()
        {
            Assert.That(cargo.RoutingStatus, Is.EqualTo(RoutingStatus.ROUTED));

            DateTime oldDepartureTime = DateTime.Parse("2008-10-24 07:00");

            Assert.That(voyage2.Schedule.DepartureTimeAt(L.NEWYORK), Is.EqualTo(oldDepartureTime));
            Assert.That(cargo.Itinerary.LoadTimeAt(L.NEWYORK), Is.EqualTo(oldDepartureTime));

            // Now voyage2 is rescheduled, the departure from NYC is delayed a few hours.
            DateTime newDepartureTime = DateTime.Parse("2008-10-24 17:00");
            voyage2.DepartureRescheduled(L.NEWYORK, newDepartureTime);

            // The schedule of voyage2 is updated
            Assert.That(voyage2.Schedule.DepartureTimeAt(L.NEWYORK), Is.EqualTo(newDepartureTime));
            // ...but the cargo itinerary still has the old departure time
            Assert.That(cargo.Itinerary.LoadTimeAt(L.NEWYORK), Is.EqualTo(oldDepartureTime));

            // Generate a new itinerary from the old one and assign the cargo to this route
            Itinerary newItinerary = cargo.Itinerary.WithRescheduledVoyage(voyage2);
            cargo.AssignToRoute(newItinerary);

            // Now the cargo aggregate is updated to reflect the scheduling change!
            Assert.That(cargo.Itinerary.LoadTimeAt(L.NEWYORK), Is.EqualTo(newDepartureTime));
            Assert.That(cargo.RoutingStatus, Is.EqualTo(RoutingStatus.ROUTED));
        }

        [Test]
        public void voyageIsRescheduledWithUnmaintainableRoute()
        {
            Assert.That(cargo.RoutingStatus, Is.EqualTo(RoutingStatus.ROUTED));

            // Voyage1 arrives in NYC at 2008-10-23 23:10
            // Now rescheduling the departure of voyage2 to BEFORE
            // voyage1 arrives in NYC. This makes it impossible to
            // keep the latter part of the old itinerary, and the new itinerary
            // is therefore truncated after unload in NYC.

            DateTime newDepartureTime = DateTime.Parse("2008-10-23 18:30");
            voyage2.DepartureRescheduled(L.NEWYORK, newDepartureTime);

            // Only the part of the itinerary up to and including NYC is maintainable, the rest is truncated
            Itinerary truncatedItinerary = cargo.Itinerary.WithRescheduledVoyage(voyage2);
            Assert.That(truncatedItinerary.LastLeg.UnloadLocation, Is.EqualTo(L.NEWYORK));

            //Or... The Itinerary is created with an 'Illegal Connection' based on a coomparison of
            //each transfer with a Location.minimumAllowedConnectionTime(). Since Loation is an entity
            //we don't allow Itinerary to dynamically use the property directly because it is not immutable.

            // The cargo enters MISROUTED state
            cargo.AssignToRoute(truncatedItinerary);
            Assert.That(cargo.RoutingStatus, Is.EqualTo(RoutingStatus.MISROUTED));
        }
    }
}