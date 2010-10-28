using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;
using V = DomainDrivenDelivery.Domain.Model.Travel.SampleVoyages;

namespace DomainDrivenDelivery.Domain.Tests.Model.Freight
{
    [TestFixture]
    public class DeliveryTest
    {
        private Delivery delivery;
        private Itinerary itinerary;
        private RouteSpecification routeSpecification;

        [SetUp]
        protected void setUp()
        {
            routeSpecification = new RouteSpecification(L.HANGZOU, L.STOCKHOLM, DateTime.Parse("2008-11-03"));
            itinerary = new Itinerary(Leg.deriveLeg(V.HONGKONG_TO_NEW_YORK, L.HANGZOU, L.NEWYORK),
                Leg.deriveLeg(V.NEW_YORK_TO_DALLAS, L.NEWYORK, L.DALLAS),
                Leg.deriveLeg(V.DALLAS_TO_HELSINKI, L.DALLAS, L.STOCKHOLM));
            delivery = Delivery.beforeHandling();
        }

        [Test]
        public void testOnHandling()
        {
            Delivery delivery = Delivery.beforeHandling();

            HandlingActivity load = HandlingActivity.loadOnto(V.HONGKONG_TO_NEW_YORK).@in(L.HONGKONG);
            delivery = delivery.onHandling(load);

            Assert.That(delivery.mostRecentHandlingActivity(), Is.EqualTo(load));
            Assert.That(delivery.mostRecentPhysicalHandlingActivity(), Is.EqualTo(load));

            HandlingActivity customs = HandlingActivity.customsIn(L.NEWYORK);
            delivery = delivery.onHandling(customs);

            Assert.That(delivery.mostRecentHandlingActivity(), Is.EqualTo(customs));
            Assert.That(delivery.mostRecentPhysicalHandlingActivity(), Is.EqualTo(load));

            HandlingActivity loadAgain = HandlingActivity.loadOnto(V.NEW_YORK_TO_DALLAS).@in(L.NEWYORK);
            delivery = delivery.onHandling(loadAgain);

            Assert.That(delivery.mostRecentHandlingActivity(), Is.EqualTo(loadAgain));
            Assert.That(delivery.mostRecentPhysicalHandlingActivity(), Is.EqualTo(loadAgain));
        }

        [Test]
        public void testDerivedFromRouteSpecificationAndItinerary()
        {
            Assert.AreEqual(RoutingStatus.ROUTED, delivery.routingStatus(itinerary, routeSpecification));
            Assert.AreEqual(Voyage.NONE, delivery.currentVoyage());
            Assert.IsFalse(delivery.isUnloadedIn(routeSpecification.destination()));
            Assert.AreEqual(Location.NONE, delivery.lastKnownLocation());
            Assert.AreEqual(TransportStatus.NOT_RECEIVED, delivery.transportStatus());
            Assert.IsTrue(delivery.lastUpdatedOn() < DateTime.Now);
        }

        [Test]
        public void testUpdateOnHandlingHappyPath()
        {
            // 1. HandlingActivityType.RECEIVE

            HandlingActivity handlingActivity = new HandlingActivity(HandlingActivityType.RECEIVE, L.HANGZOU);
            Delivery newDelivery = delivery.onHandling(handlingActivity);

            // Changed on handling
            Assert.AreEqual(Voyage.NONE, newDelivery.currentVoyage());
            Assert.AreEqual(L.HANGZOU, newDelivery.lastKnownLocation());
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.transportStatus());

            // Changed on handling and/or (re-)routing
            Assert.IsFalse(newDelivery.isUnloadedIn(routeSpecification.destination()));

            // Changed on (re-)routing
            Assert.AreEqual(RoutingStatus.ROUTED, newDelivery.routingStatus(itinerary, routeSpecification));

            // Updated on every calculation
            Assert.IsTrue(delivery.lastUpdatedOn() < (newDelivery.lastUpdatedOn()));

            // 2. Load

            handlingActivity = new HandlingActivity(HandlingActivityType.LOAD, L.HANGZOU, V.HONGKONG_TO_NEW_YORK);
            newDelivery = newDelivery.onHandling(handlingActivity);

            Assert.AreEqual(V.HONGKONG_TO_NEW_YORK, newDelivery.currentVoyage());
            Assert.AreEqual(L.HANGZOU, newDelivery.lastKnownLocation());
            Assert.AreEqual(TransportStatus.ONBOARD_CARRIER, newDelivery.transportStatus());

            Assert.IsFalse(newDelivery.isUnloadedIn(routeSpecification.destination()));

            Assert.AreEqual(RoutingStatus.ROUTED, newDelivery.routingStatus(itinerary, routeSpecification));

            Assert.IsTrue(delivery.lastUpdatedOn() < (newDelivery.lastUpdatedOn()));

            // Skipping intermediate load/unloads

            // 3. Unload

            handlingActivity = new HandlingActivity(HandlingActivityType.UNLOAD, L.STOCKHOLM, V.DALLAS_TO_HELSINKI);
            newDelivery = newDelivery.onHandling(handlingActivity);

            Assert.AreEqual(Voyage.NONE, newDelivery.currentVoyage());
            Assert.AreEqual(L.STOCKHOLM, newDelivery.lastKnownLocation());
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.transportStatus());

            Assert.IsTrue(newDelivery.isUnloadedIn(routeSpecification.destination()));

            Assert.AreEqual(RoutingStatus.ROUTED, newDelivery.routingStatus(itinerary, routeSpecification));

            Assert.IsTrue(delivery.lastUpdatedOn() < (newDelivery.lastUpdatedOn()));

            // 4. Claim

            handlingActivity = new HandlingActivity(HandlingActivityType.CLAIM, L.STOCKHOLM);
            newDelivery = newDelivery.onHandling(handlingActivity);

            Assert.AreEqual(Voyage.NONE, newDelivery.currentVoyage());
            Assert.AreEqual(L.STOCKHOLM, newDelivery.lastKnownLocation());
            Assert.AreEqual(TransportStatus.CLAIMED, newDelivery.transportStatus());

            Assert.IsFalse(newDelivery.isUnloadedIn(routeSpecification.destination()));

            Assert.AreEqual(RoutingStatus.ROUTED, newDelivery.routingStatus(itinerary, routeSpecification));

            Assert.IsTrue(delivery.lastUpdatedOn() < (newDelivery.lastUpdatedOn()));
        }

        [Test]
        public void testUpdateOnHandlingWhenMisdirected()
        {
            // Unload in L.HAMBURG, which is the wrong location
            HandlingActivity handlingActivity = new HandlingActivity(HandlingActivityType.UNLOAD,
                L.HAMBURG,
                V.DALLAS_TO_HELSINKI);
            Delivery newDelivery = delivery.onHandling(handlingActivity);

            Assert.AreEqual(Voyage.NONE, newDelivery.currentVoyage());
            Assert.AreEqual(L.HAMBURG, newDelivery.lastKnownLocation());
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.transportStatus());

            // Next handling activity is undefined. Need a new itinerary to know what to do.

            Assert.IsFalse(newDelivery.isUnloadedIn(routeSpecification.destination()));

            Assert.AreEqual(RoutingStatus.ROUTED, newDelivery.routingStatus(itinerary, routeSpecification));

            Assert.IsTrue(delivery.lastUpdatedOn() < (newDelivery.lastUpdatedOn()));

            // New route specification, old itinerary
            RouteSpecification newRouteSpecification = routeSpecification.withOrigin(L.HAMBURG);
            Assert.AreEqual(RoutingStatus.MISROUTED, newDelivery.routingStatus(itinerary, newRouteSpecification));

            Itinerary newItinerary = new Itinerary(Leg.deriveLeg(V.DALLAS_TO_HELSINKI, L.HAMBURG, L.STOCKHOLM));

            Assert.AreEqual(RoutingStatus.ROUTED, newDelivery.routingStatus(newItinerary, newRouteSpecification));
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.transportStatus());
        }

        [Test]
        public void testEmptyCtor()
        {
            new Delivery();
        }
    }
}