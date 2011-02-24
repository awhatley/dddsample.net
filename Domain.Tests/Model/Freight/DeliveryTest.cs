using System;
using System.Threading;

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
            itinerary = new Itinerary(
                Leg.DeriveLeg(V.HONGKONG_TO_NEW_YORK, L.HANGZOU, L.NEWYORK),
                Leg.DeriveLeg(V.NEW_YORK_TO_DALLAS, L.NEWYORK, L.DALLAS),
                Leg.DeriveLeg(V.DALLAS_TO_HELSINKI, L.DALLAS, L.STOCKHOLM));
            delivery = Delivery.BeforeHandling();
            Thread.Sleep(1);
        }

        [Test]
        public void testOnHandling()
        {
            Delivery delivery = Delivery.BeforeHandling();

            HandlingActivity load = HandlingActivity.LoadOnto(V.HONGKONG_TO_NEW_YORK).In(L.HONGKONG);
            delivery = delivery.OnHandling(load);

            Assert.That(delivery.MostRecentHandlingActivity, Is.EqualTo(load));
            Assert.That(delivery.MostRecentPhysicalHandlingActivity, Is.EqualTo(load));

            HandlingActivity customs = HandlingActivity.CustomsIn(L.NEWYORK);
            delivery = delivery.OnHandling(customs);

            Assert.That(delivery.MostRecentHandlingActivity, Is.EqualTo(customs));
            Assert.That(delivery.MostRecentPhysicalHandlingActivity, Is.EqualTo(load));

            HandlingActivity loadAgain = HandlingActivity.LoadOnto(V.NEW_YORK_TO_DALLAS).In(L.NEWYORK);
            delivery = delivery.OnHandling(loadAgain);

            Assert.That(delivery.MostRecentHandlingActivity, Is.EqualTo(loadAgain));
            Assert.That(delivery.MostRecentPhysicalHandlingActivity, Is.EqualTo(loadAgain));
        }

        [Test]
        public void testDerivedFromRouteSpecificationAndItinerary()
        {
            Assert.AreEqual(RoutingStatus.ROUTED, routeSpecification.StatusOf(itinerary));
            Assert.AreEqual(Voyage.None, delivery.CurrentVoyage);
            Assert.IsFalse(delivery.IsUnloadedIn(routeSpecification.Destination));
            Assert.AreEqual(Location.None, delivery.LastKnownLocation);
            Assert.AreEqual(TransportStatus.NOT_RECEIVED, delivery.TransportStatus);
            Assert.That(delivery.LastUpdatedOn, Is.InRange(DateTime.Now.AddSeconds(-1), DateTime.Now.AddSeconds(1)));
        }

        [Test]
        public void testUpdateOnHandlingHappyPath()
        {
            // 1. HandlingActivityType.RECEIVE

            HandlingActivity handlingActivity = new HandlingActivity(HandlingActivityType.RECEIVE, L.HANGZOU);
            Delivery newDelivery = delivery.OnHandling(handlingActivity);

            // Changed on handling
            Assert.AreEqual(Voyage.None, newDelivery.CurrentVoyage);
            Assert.AreEqual(L.HANGZOU, newDelivery.LastKnownLocation);
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.TransportStatus);

            // Changed on handling and/or (re-)routing
            Assert.IsFalse(newDelivery.IsUnloadedIn(routeSpecification.Destination));

            // Changed on (re-)routing
            Assert.AreEqual(RoutingStatus.ROUTED, routeSpecification.StatusOf(itinerary));

            // Updated on every calculation
            Assert.IsTrue(delivery.LastUpdatedOn < (newDelivery.LastUpdatedOn));

            // 2. Load

            handlingActivity = new HandlingActivity(HandlingActivityType.LOAD, L.HANGZOU, V.HONGKONG_TO_NEW_YORK);
            newDelivery = newDelivery.OnHandling(handlingActivity);

            Assert.AreEqual(V.HONGKONG_TO_NEW_YORK, newDelivery.CurrentVoyage);
            Assert.AreEqual(L.HANGZOU, newDelivery.LastKnownLocation);
            Assert.AreEqual(TransportStatus.ONBOARD_CARRIER, newDelivery.TransportStatus);

            Assert.IsFalse(newDelivery.IsUnloadedIn(routeSpecification.Destination));

            Assert.AreEqual(RoutingStatus.ROUTED, routeSpecification.StatusOf(itinerary));

            Assert.IsTrue(delivery.LastUpdatedOn < (newDelivery.LastUpdatedOn));

            // Skipping intermediate load/unloads

            // 3. Unload

            handlingActivity = new HandlingActivity(HandlingActivityType.UNLOAD, L.STOCKHOLM, V.DALLAS_TO_HELSINKI);
            newDelivery = newDelivery.OnHandling(handlingActivity);

            Assert.AreEqual(Voyage.None, newDelivery.CurrentVoyage);
            Assert.AreEqual(L.STOCKHOLM, newDelivery.LastKnownLocation);
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.TransportStatus);

            Assert.IsTrue(newDelivery.IsUnloadedIn(routeSpecification.Destination));

            Assert.AreEqual(RoutingStatus.ROUTED, routeSpecification.StatusOf(itinerary));

            Assert.IsTrue(delivery.LastUpdatedOn < (newDelivery.LastUpdatedOn));

            // 4. Claim

            handlingActivity = new HandlingActivity(HandlingActivityType.CLAIM, L.STOCKHOLM);
            newDelivery = newDelivery.OnHandling(handlingActivity);

            Assert.AreEqual(Voyage.None, newDelivery.CurrentVoyage);
            Assert.AreEqual(L.STOCKHOLM, newDelivery.LastKnownLocation);
            Assert.AreEqual(TransportStatus.CLAIMED, newDelivery.TransportStatus);

            Assert.IsFalse(newDelivery.IsUnloadedIn(routeSpecification.Destination));

            Assert.AreEqual(RoutingStatus.ROUTED, routeSpecification.StatusOf(itinerary));

            Assert.IsTrue(delivery.LastUpdatedOn < (newDelivery.LastUpdatedOn));
        }

        [Test]
        public void testUpdateOnHandlingWhenMisdirected()
        {
            // Unload in L.HAMBURG, which is the wrong location
            HandlingActivity handlingActivity = new HandlingActivity(HandlingActivityType.UNLOAD,
                L.HAMBURG,
                V.DALLAS_TO_HELSINKI);
            Delivery newDelivery = delivery.OnHandling(handlingActivity);

            Assert.AreEqual(Voyage.None, newDelivery.CurrentVoyage);
            Assert.AreEqual(L.HAMBURG, newDelivery.LastKnownLocation);
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.TransportStatus);

            // Next handling activity is undefined. Need a new itinerary to know what to do.

            Assert.IsFalse(newDelivery.IsUnloadedIn(routeSpecification.Destination));

            Assert.AreEqual(RoutingStatus.ROUTED, routeSpecification.StatusOf(itinerary));

            Assert.IsTrue(delivery.LastUpdatedOn < (newDelivery.LastUpdatedOn));

            // New route specification, old itinerary
            RouteSpecification newRouteSpecification = routeSpecification.WithOrigin(L.HAMBURG);
            Assert.AreEqual(RoutingStatus.MISROUTED, newRouteSpecification.StatusOf(itinerary));

            Itinerary newItinerary = new Itinerary(Leg.DeriveLeg(V.DALLAS_TO_HELSINKI, L.HAMBURG, L.STOCKHOLM));

            Assert.AreEqual(RoutingStatus.ROUTED, newRouteSpecification.StatusOf(newItinerary));
            Assert.AreEqual(TransportStatus.IN_PORT, newDelivery.TransportStatus);
        }

        [Test]
        public void testEmptyCtor()
        {
            new Delivery();
        }
    }
}