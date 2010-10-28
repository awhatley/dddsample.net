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
    public class CargoTest
    {
        private Voyage crazyVoyage =
            new Voyage.Builder(new VoyageNumber("0123"), L.STOCKHOLM).addMovement(L.HAMBURG,
                new DateTime(1),
                new DateTime(2)).addMovement(L.HONGKONG, new DateTime(3), new DateTime(4)).addMovement(L.MELBOURNE,
                    new DateTime(5),
                    new DateTime(6)).build();

        private Voyage pacific =
            new Voyage.Builder(new VoyageNumber("4567"), L.SHANGHAI).addMovement(L.LONGBEACH,
                new DateTime(1),
                new DateTime(2)).addMovement(L.SEATTLE, new DateTime(3), new DateTime(4)).build();

        private Voyage transcontinental =
            new Voyage.Builder(new VoyageNumber("4567"), L.LONGBEACH).addMovement(L.CHICAGO,
                new DateTime(1),
                new DateTime(2)).addMovement(L.NEWYORK, new DateTime(3), new DateTime(4)).build();

        private Voyage northernRail =
            new Voyage.Builder(new VoyageNumber("8901"), L.SEATTLE).addMovement(L.CHICAGO,
                new DateTime(1),
                new DateTime(2)).addMovement(L.NEWYORK, new DateTime(3), new DateTime(4)).build();

        [Test]
        public void testConstruction()
        {
            TrackingId trackingId = new TrackingId("XYZ");
            DateTime arrivalDeadline = DateTime.Parse("2009-03-13");
            RouteSpecification routeSpecification = new RouteSpecification(L.STOCKHOLM, L.MELBOURNE, arrivalDeadline);

            Cargo cargo = new Cargo(trackingId, routeSpecification);

            Assert.AreEqual(RoutingStatus.NOT_ROUTED, cargo.routingStatus());
            Assert.AreEqual(TransportStatus.NOT_RECEIVED, cargo.transportStatus());
            Assert.AreEqual(Location.NONE, cargo.lastKnownLocation());
            Assert.AreEqual(Voyage.NONE, cargo.currentVoyage());
        }

        [Test]
        public void testEmptyCtor()
        {
            new Cargo();
        }

        [Test]
        public void testRoutingStatus()
        {
            Cargo cargo = new Cargo(new TrackingId("XYZ"),
                new RouteSpecification(L.STOCKHOLM, L.MELBOURNE, DateTime.Now));
            Itinerary good = new Itinerary(Leg.deriveLeg(northernRail, L.SEATTLE, L.NEWYORK));
            Itinerary bad = new Itinerary(Leg.deriveLeg(crazyVoyage, L.HAMBURG, L.HONGKONG));
            RouteSpecification acceptOnlyGood = new RouteSpecification(L.SEATTLE, L.NEWYORK, DateTime.Now);

            cargo.specifyNewRoute(acceptOnlyGood);
            Assert.AreEqual(RoutingStatus.NOT_ROUTED, cargo.routingStatus());

            cargo.assignToRoute(bad);
            Assert.AreEqual(RoutingStatus.MISROUTED, cargo.routingStatus());

            cargo.assignToRoute(good);
            Assert.AreEqual(RoutingStatus.ROUTED, cargo.routingStatus());
        }

        [Test]
        public void testOutOrderHandling()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.handled(HandlingActivity.loadOnto(crazyVoyage).@in(L.STOCKHOLM));
            Assert.That(cargo.transportStatus(), Is.EqualTo(TransportStatus.ONBOARD_CARRIER));
            Assert.That(cargo.lastKnownLocation(), Is.EqualTo(L.STOCKHOLM));

            cargo.handled(HandlingActivity.unloadOff(crazyVoyage).@in(L.HAMBURG));
            Assert.That(cargo.transportStatus(), Is.EqualTo(TransportStatus.IN_PORT));
            Assert.That(cargo.lastKnownLocation(), Is.EqualTo(L.HAMBURG));

            cargo.handled(HandlingActivity.unloadOff(crazyVoyage).@in(L.MELBOURNE));
            Assert.That(cargo.transportStatus(), Is.EqualTo(TransportStatus.IN_PORT));
            Assert.That(cargo.lastKnownLocation(), Is.EqualTo(L.MELBOURNE));

            // Out of order handling, does not affect state of cargo
            cargo.handled(HandlingActivity.loadOnto(crazyVoyage).@in(L.HAMBURG));
            Assert.That(cargo.transportStatus(), Is.EqualTo(TransportStatus.IN_PORT));
            Assert.That(cargo.lastKnownLocation(), Is.EqualTo(L.MELBOURNE));
        }

        [Test]
        public void testlastKnownLocationUnknownWhenNoEvents()
        {
            Cargo cargo = new Cargo(new TrackingId("XYZ"),
                new RouteSpecification(L.STOCKHOLM, L.MELBOURNE, DateTime.Now));

            Assert.AreEqual(Location.NONE, cargo.lastKnownLocation());
        }

        [Test]
        public void testlastKnownLocationReceived()
        {
            Cargo cargo = populateCargoReceivedStockholm();

            Assert.AreEqual(L.STOCKHOLM, cargo.lastKnownLocation());
        }

        [Test]
        public void testlastKnownLocationClaimed()
        {
            Cargo cargo = populateCargoClaimedMelbourne();

            Assert.AreEqual(L.MELBOURNE, cargo.lastKnownLocation());
        }

        [Test]
        public void testlastKnownLocationUnloaded()
        {
            Cargo cargo = populateCargoOffHongKong();

            Assert.AreEqual(L.MELBOURNE, cargo.lastKnownLocation());
        }

        [Test]
        public void testlastKnownLocationloaded()
        {
            Cargo cargo = populateCargoOnHamburg();

            Assert.AreEqual(L.HAMBURG, cargo.lastKnownLocation());
        }

        [Test]
        public void testEquality()
        {
            RouteSpecification spec1 = new RouteSpecification(L.STOCKHOLM, L.HONGKONG, DateTime.Now);
            RouteSpecification spec2 = new RouteSpecification(L.STOCKHOLM, L.MELBOURNE, DateTime.Now);
            Cargo c1 = new Cargo(new TrackingId("ABC"), spec1);
            Cargo c2 = new Cargo(new TrackingId("CBA"), spec1);
            Cargo c3 = new Cargo(new TrackingId("ABC"), spec2);
            Cargo c4 = new Cargo(new TrackingId("ABC"), spec1);

            Assert.IsTrue(c1.Equals(c4), "Cargos should be equal when TrackingIDs are equal");
            Assert.IsTrue(c1.Equals(c3), "Cargos should be equal when TrackingIDs are equal");
            Assert.IsTrue(c3.Equals(c4), "Cargos should be equal when TrackingIDs are equal");
            Assert.IsFalse(c1.Equals(c2), "Cargos are not equal when TrackingID differ");
        }

        [Test]
        public void testIsReadyToClaimWithDestinationDifferentFromCustomsClearancePoint()
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"),
                new RouteSpecification(L.HONGKONG, L.NEWYORK, DateTime.Parse("2009-12-24")));
            Itinerary itinerary = new Itinerary(Leg.deriveLeg(V.pacific1, L.HONGKONG, L.LONGBEACH),
                Leg.deriveLeg(V.continental2, L.LONGBEACH, L.NEWYORK));
            cargo.assignToRoute(itinerary);
            Assert.IsFalse(cargo.routeSpecification().destination().sameAs(cargo.customsClearancePoint()));
            Assert.IsFalse(cargo.isReadyToClaim());

            cargo.handled(HandlingActivity.unloadOff(V.pacific1).@in(L.LONGBEACH));
            Assert.IsFalse(cargo.isReadyToClaim());

            cargo.handled(HandlingActivity.loadOnto(V.continental2).@in(L.LONGBEACH));
            Assert.IsFalse(cargo.isReadyToClaim());

            cargo.handled(HandlingActivity.unloadOff(V.continental2).@in(L.NEWYORK));
            Assert.IsTrue(cargo.isReadyToClaim());
        }

        [Test]
        public void testIsReadyToClaimWithDestinationSameAsCustomsClearancePoint()
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"),
                new RouteSpecification(L.SHANGHAI, L.SEATTLE, DateTime.Parse("2009-12-24")));
            Itinerary itinerary = new Itinerary(Leg.deriveLeg(V.pacific2, L.SHANGHAI, L.SEATTLE));
            cargo.assignToRoute(itinerary);
            Assert.IsTrue(cargo.routeSpecification().destination().sameAs(cargo.customsClearancePoint()));
            Assert.IsFalse(cargo.isReadyToClaim());

            cargo.handled(HandlingActivity.unloadOff(V.pacific2).@in(L.SEATTLE));
            Assert.IsFalse(cargo.isReadyToClaim());

            cargo.handled(HandlingActivity.customsIn(L.SEATTLE));
            Assert.IsTrue(cargo.isReadyToClaim());

            cargo.handled(HandlingActivity.claimIn(L.SEATTLE));
            Assert.IsFalse(cargo.isReadyToClaim());
        }

        [Test]
        public void testIsMisdirectedHappyPath()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            //A cargo with no handling events is not misdirected
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.receiveIn(L.SHANGHAI));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.loadOnto(V.pacific2).@in(L.SHANGHAI));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.unloadOff(V.pacific2).@in(L.SEATTLE));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.customsIn(L.SEATTLE));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.loadOnto(V.continental3).@in(L.SEATTLE));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.unloadOff(V.continental3).@in(L.CHICAGO));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.claimIn(L.CHICAGO));
            Assert.IsFalse(cargo.isMisdirected());
        }

        [Test]
        public void testIsMisdirectedIncorrectReceive()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.handled(HandlingActivity.receiveIn(L.TOKYO));
            Assert.IsTrue(cargo.isMisdirected());
        }

        [Test]
        public void testIsMisdirectedLoadOntoWrongVoyage()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.handled(HandlingActivity.loadOnto(V.pacific1).@in(L.HONGKONG));
            Assert.IsTrue(cargo.isMisdirected());
        }

        [Test]
        public void testIsMisdirectedUnloadInWrongLocation()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.handled(HandlingActivity.unloadOff(V.pacific2).@in(L.TOKYO));
            Assert.IsTrue(cargo.isMisdirected());
        }

        [Test]
        public void testIsMisdirectedCustomsInWrongLocation()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.handled(HandlingActivity.customsIn(L.CHICAGO));
            Assert.IsTrue(cargo.isMisdirected());
        }

        [Test]
        public void testIsMisdirectedClaimedInWrongLocation()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.handled(HandlingActivity.claimIn(L.SEATTLE));
            Assert.IsTrue(cargo.isMisdirected());
        }

        [Test]
        public void testIsMisdirectedAfterRerouting()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.handled(HandlingActivity.loadOnto(V.pacific2).@in(L.SHANGHAI));
            Assert.IsFalse(cargo.isMisdirected());

            // Cargo destination is changed by customer mid-route
            RouteSpecification newRouteSpec =
                cargo.routeSpecification().withOrigin(cargo.lastKnownLocation()).withDestination(L.NEWYORK);

            cargo.specifyNewRoute(newRouteSpec);
            // Misrouted, but not misdirected. Delivery is still accoring to plan (itinerary),
            // but not according to desire (route specification).
            Assert.IsFalse(cargo.isMisdirected());
            Assert.IsTrue(cargo.routingStatus() == RoutingStatus.MISROUTED);

            /**
     * This is a perfect example of how LegActivityMatch is a modelling breakthrough.
     * It allows us to easily construct an itinerary that completes the remainder of the
     * old itinerary and appends the new and different path.
     */
            Leg currentLeg = cargo.itinerary().matchLeg(cargo.mostRecentHandlingActivity()).leg();
            Itinerary newItinerary = new Itinerary(currentLeg, Leg.deriveLeg(V.continental3, L.SEATTLE, L.NEWYORK));
            cargo.assignToRoute(newItinerary);
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.unloadOff(V.pacific2).@in(L.SEATTLE));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.loadOnto(V.continental3).@in(L.SEATTLE));
            Assert.IsFalse(cargo.isMisdirected());

            cargo.handled(HandlingActivity.unloadOff(V.continental3).@in(L.NEWYORK));
            Assert.IsFalse(cargo.isMisdirected());
        }

        [Test]
        public void testCustomsClearancePoint()
        {
            //cargo destination NYC
            Cargo cargo = new Cargo(new TrackingId("XYZ"), new RouteSpecification(L.SHANGHAI, L.NEWYORK, DateTime.Now));

            Assert.That(cargo.customsClearancePoint(), Is.EqualTo(Location.NONE));

            //SHA-LGB-NYC
            cargo.assignToRoute(new Itinerary(Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH),
                Leg.deriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK)));
            Assert.AreEqual(L.LONGBEACH, cargo.customsClearancePoint());

            //SHA-SEA-NYC
            cargo.assignToRoute(new Itinerary(Leg.deriveLeg(pacific, L.SHANGHAI, L.SEATTLE),
                Leg.deriveLeg(northernRail, L.SEATTLE, L.NEWYORK)));
            Assert.AreEqual(L.SEATTLE, cargo.customsClearancePoint());

            //cargo destination LGB
            //SHA-LGB
            cargo.specifyNewRoute(new RouteSpecification(L.SHANGHAI, L.LONGBEACH, DateTime.Now));
            cargo.assignToRoute(new Itinerary(Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH)));
            Assert.AreEqual(L.LONGBEACH, cargo.customsClearancePoint());

            //Cargo destination HAMBURG
            //SHA-LGB-NYC This itinerary does not take
            // the cargo into its CustomsZone, so no clearancePoint.
            cargo.specifyNewRoute(new RouteSpecification(L.SHANGHAI, L.HAMBURG, DateTime.Now));
            cargo.assignToRoute(new Itinerary(Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH),
                Leg.deriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK)));
            Assert.IsNull(cargo.customsClearancePoint());

            //Cargo destination NEWYORK on SHA-LGB-CHI
            //This itinerary does not take the cargo to its destination,
            //but it does enter the CustomsZone, so it has a clearancePoint.
            cargo.specifyNewRoute(new RouteSpecification(L.SHANGHAI, L.NEWYORK, DateTime.Now));
            cargo.assignToRoute(new Itinerary(Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH),
                Leg.deriveLeg(transcontinental, L.LONGBEACH, L.CHICAGO)));
            Assert.AreEqual(L.LONGBEACH, cargo.customsClearancePoint());
        }

        private Cargo shanghaiSeattleChicagoOnPacific2AndContinental3()
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"),
                new RouteSpecification(L.SHANGHAI, L.CHICAGO, DateTime.Parse("2009-12-24")));

            // A cargo with no itinerary is not misdirected
            Assert.IsFalse(cargo.isMisdirected());

            Itinerary itinerary = new Itinerary(Leg.deriveLeg(V.pacific2, L.SHANGHAI, L.SEATTLE),
                Leg.deriveLeg(V.continental3, L.SEATTLE, L.CHICAGO));
            cargo.assignToRoute(itinerary);
            return cargo;
        }

        private Cargo setUpCargoWithItinerary(Location origin, Location midpoint, Location destination)
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"), new RouteSpecification(origin, destination, DateTime.Now));

            Itinerary itinerary = new Itinerary(Leg.deriveLeg(crazyVoyage, origin, midpoint),
                Leg.deriveLeg(crazyVoyage, midpoint, destination));

            cargo.assignToRoute(itinerary);
            return cargo;
        }

        private Cargo populateCargoReceivedStockholm()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);
            cargo.handled(new HandlingActivity(HandlingActivityType.RECEIVE, L.STOCKHOLM));
            return cargo;
        }

        private Cargo populateCargoClaimedMelbourne()
        {
            Cargo cargo = populateCargoOffMelbourne();

            cargo.handled(new HandlingActivity(HandlingActivityType.CLAIM, L.MELBOURNE));
            return cargo;
        }

        private Cargo populateCargoOffHongKong()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.handled(new HandlingActivity(HandlingActivityType.LOAD, L.STOCKHOLM, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.HAMBURG, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.LOAD, L.HAMBURG, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.MELBOURNE, crazyVoyage));
            return cargo;
        }

        private Cargo populateCargoOnHamburg()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.handled(new HandlingActivity(HandlingActivityType.LOAD, L.STOCKHOLM, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.HAMBURG, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.LOAD, L.HAMBURG, crazyVoyage));
            return cargo;
        }

        private Cargo populateCargoOffMelbourne()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.handled(new HandlingActivity(HandlingActivityType.LOAD, L.STOCKHOLM, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.HAMBURG, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.LOAD, L.HAMBURG, crazyVoyage));
            cargo.handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.MELBOURNE, crazyVoyage));

            return cargo;
        }
    }
}