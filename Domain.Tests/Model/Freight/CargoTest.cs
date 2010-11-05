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

            Assert.AreEqual(RoutingStatus.NOT_ROUTED, cargo.RoutingStatus);
            Assert.AreEqual(TransportStatus.NOT_RECEIVED, cargo.TransportStatus);
            Assert.AreEqual(Location.None, cargo.LastKnownLocation);
            Assert.AreEqual(Voyage.None, cargo.CurrentVoyage);
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
            Itinerary good = new Itinerary(Leg.DeriveLeg(northernRail, L.SEATTLE, L.NEWYORK));
            Itinerary bad = new Itinerary(Leg.DeriveLeg(crazyVoyage, L.HAMBURG, L.HONGKONG));
            RouteSpecification acceptOnlyGood = new RouteSpecification(L.SEATTLE, L.NEWYORK, DateTime.Now);

            cargo.SpecifyNewRoute(acceptOnlyGood);
            Assert.AreEqual(RoutingStatus.NOT_ROUTED, cargo.RoutingStatus);

            cargo.AssignToRoute(bad);
            Assert.AreEqual(RoutingStatus.MISROUTED, cargo.RoutingStatus);

            cargo.AssignToRoute(good);
            Assert.AreEqual(RoutingStatus.ROUTED, cargo.RoutingStatus);
        }

        [Test]
        public void testOutOrderHandling()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.Handled(HandlingActivity.LoadOnto(crazyVoyage).In(L.STOCKHOLM));
            Assert.That(cargo.TransportStatus, Is.EqualTo(TransportStatus.ONBOARD_CARRIER));
            Assert.That(cargo.LastKnownLocation, Is.EqualTo(L.STOCKHOLM));

            cargo.Handled(HandlingActivity.UnloadOff(crazyVoyage).In(L.HAMBURG));
            Assert.That(cargo.TransportStatus, Is.EqualTo(TransportStatus.IN_PORT));
            Assert.That(cargo.LastKnownLocation, Is.EqualTo(L.HAMBURG));

            cargo.Handled(HandlingActivity.UnloadOff(crazyVoyage).In(L.MELBOURNE));
            Assert.That(cargo.TransportStatus, Is.EqualTo(TransportStatus.IN_PORT));
            Assert.That(cargo.LastKnownLocation, Is.EqualTo(L.MELBOURNE));

            // Out of order handling, does not affect state of cargo
            cargo.Handled(HandlingActivity.LoadOnto(crazyVoyage).In(L.HAMBURG));
            Assert.That(cargo.TransportStatus, Is.EqualTo(TransportStatus.IN_PORT));
            Assert.That(cargo.LastKnownLocation, Is.EqualTo(L.MELBOURNE));
        }

        [Test]
        public void testlastKnownLocationUnknownWhenNoEvents()
        {
            Cargo cargo = new Cargo(new TrackingId("XYZ"),
                new RouteSpecification(L.STOCKHOLM, L.MELBOURNE, DateTime.Now));

            Assert.AreEqual(Location.None, cargo.LastKnownLocation);
        }

        [Test]
        public void testlastKnownLocationReceived()
        {
            Cargo cargo = populateCargoReceivedStockholm();

            Assert.AreEqual(L.STOCKHOLM, cargo.LastKnownLocation);
        }

        [Test]
        public void testlastKnownLocationClaimed()
        {
            Cargo cargo = populateCargoClaimedMelbourne();

            Assert.AreEqual(L.MELBOURNE, cargo.LastKnownLocation);
        }

        [Test]
        public void testlastKnownLocationUnloaded()
        {
            Cargo cargo = populateCargoOffHongKong();

            Assert.AreEqual(L.MELBOURNE, cargo.LastKnownLocation);
        }

        [Test]
        public void testlastKnownLocationloaded()
        {
            Cargo cargo = populateCargoOnHamburg();

            Assert.AreEqual(L.HAMBURG, cargo.LastKnownLocation);
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
            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(V.pacific1, L.HONGKONG, L.LONGBEACH),
                Leg.DeriveLeg(V.continental2, L.LONGBEACH, L.NEWYORK));
            cargo.AssignToRoute(itinerary);
            Assert.IsFalse(cargo.RouteSpecification.Destination.sameAs(cargo.CustomsClearancePoint));
            Assert.IsFalse(cargo.IsReadyToClaim);

            cargo.Handled(HandlingActivity.UnloadOff(V.pacific1).In(L.LONGBEACH));
            Assert.IsFalse(cargo.IsReadyToClaim);

            cargo.Handled(HandlingActivity.LoadOnto(V.continental2).In(L.LONGBEACH));
            Assert.IsFalse(cargo.IsReadyToClaim);

            cargo.Handled(HandlingActivity.UnloadOff(V.continental2).In(L.NEWYORK));
            Assert.IsTrue(cargo.IsReadyToClaim);
        }

        [Test]
        public void testIsReadyToClaimWithDestinationSameAsCustomsClearancePoint()
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"),
                new RouteSpecification(L.SHANGHAI, L.SEATTLE, DateTime.Parse("2009-12-24")));
            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(V.pacific2, L.SHANGHAI, L.SEATTLE));
            cargo.AssignToRoute(itinerary);
            Assert.IsTrue(cargo.RouteSpecification.Destination.sameAs(cargo.CustomsClearancePoint));
            Assert.IsFalse(cargo.IsReadyToClaim);

            cargo.Handled(HandlingActivity.UnloadOff(V.pacific2).In(L.SEATTLE));
            Assert.IsFalse(cargo.IsReadyToClaim);

            cargo.Handled(HandlingActivity.CustomsIn(L.SEATTLE));
            Assert.IsTrue(cargo.IsReadyToClaim);

            cargo.Handled(HandlingActivity.ClaimIn(L.SEATTLE));
            Assert.IsFalse(cargo.IsReadyToClaim);
        }

        [Test]
        public void testIsMisdirectedHappyPath()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            //A cargo with no handling events is not misdirected
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.ReceiveIn(L.SHANGHAI));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.LoadOnto(V.pacific2).In(L.SHANGHAI));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.UnloadOff(V.pacific2).In(L.SEATTLE));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.CustomsIn(L.SEATTLE));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.LoadOnto(V.continental3).In(L.SEATTLE));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.UnloadOff(V.continental3).In(L.CHICAGO));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.ClaimIn(L.CHICAGO));
            Assert.IsFalse(cargo.IsMisdirected);
        }

        [Test]
        public void testIsMisdirectedIncorrectReceive()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.Handled(HandlingActivity.ReceiveIn(L.TOKYO));
            Assert.IsTrue(cargo.IsMisdirected);
        }

        [Test]
        public void testIsMisdirectedLoadOntoWrongVoyage()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.Handled(HandlingActivity.LoadOnto(V.pacific1).In(L.HONGKONG));
            Assert.IsTrue(cargo.IsMisdirected);
        }

        [Test]
        public void testIsMisdirectedUnloadInWrongLocation()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.Handled(HandlingActivity.UnloadOff(V.pacific2).In(L.TOKYO));
            Assert.IsTrue(cargo.IsMisdirected);
        }

        [Test]
        public void testIsMisdirectedCustomsInWrongLocation()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.Handled(HandlingActivity.CustomsIn(L.CHICAGO));
            Assert.IsTrue(cargo.IsMisdirected);
        }

        [Test]
        public void testIsMisdirectedClaimedInWrongLocation()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.Handled(HandlingActivity.ClaimIn(L.SEATTLE));
            Assert.IsTrue(cargo.IsMisdirected);
        }

        [Test]
        public void testIsMisdirectedAfterRerouting()
        {
            Cargo cargo = shanghaiSeattleChicagoOnPacific2AndContinental3();

            cargo.Handled(HandlingActivity.LoadOnto(V.pacific2).In(L.SHANGHAI));
            Assert.IsFalse(cargo.IsMisdirected);

            // Cargo destination is changed by customer mid-route
            RouteSpecification newRouteSpec =
                cargo.RouteSpecification.WithOrigin(cargo.LastKnownLocation).WithDestination(L.NEWYORK);

            cargo.SpecifyNewRoute(newRouteSpec);
            // Misrouted, but not misdirected. Delivery is still accoring to plan (itinerary),
            // but not according to desire (route specification).
            Assert.IsFalse(cargo.IsMisdirected);
            Assert.IsTrue(cargo.RoutingStatus == RoutingStatus.MISROUTED);

            /**
     * This is a perfect example of how LegActivityMatch is a modelling breakthrough.
     * It allows us to easily construct an itinerary that completes the remainder of the
     * old itinerary and appends the new and different path.
     */
            Leg currentLeg = cargo.Itinerary.MatchLeg(cargo.MostRecentHandlingActivity).Leg;
            Itinerary newItinerary = new Itinerary(currentLeg, Leg.DeriveLeg(V.continental3, L.SEATTLE, L.NEWYORK));
            cargo.AssignToRoute(newItinerary);
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.UnloadOff(V.pacific2).In(L.SEATTLE));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.LoadOnto(V.continental3).In(L.SEATTLE));
            Assert.IsFalse(cargo.IsMisdirected);

            cargo.Handled(HandlingActivity.UnloadOff(V.continental3).In(L.NEWYORK));
            Assert.IsFalse(cargo.IsMisdirected);
        }

        [Test]
        public void testCustomsClearancePoint()
        {
            //cargo destination NYC
            Cargo cargo = new Cargo(new TrackingId("XYZ"), new RouteSpecification(L.SHANGHAI, L.NEWYORK, DateTime.Now));

            Assert.That(cargo.CustomsClearancePoint, Is.EqualTo(Location.None));

            //SHA-LGB-NYC
            cargo.AssignToRoute(new Itinerary(Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH),
                Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK)));
            Assert.AreEqual(L.LONGBEACH, cargo.CustomsClearancePoint);

            //SHA-SEA-NYC
            cargo.AssignToRoute(new Itinerary(Leg.DeriveLeg(pacific, L.SHANGHAI, L.SEATTLE),
                Leg.DeriveLeg(northernRail, L.SEATTLE, L.NEWYORK)));
            Assert.AreEqual(L.SEATTLE, cargo.CustomsClearancePoint);

            //cargo destination LGB
            //SHA-LGB
            cargo.SpecifyNewRoute(new RouteSpecification(L.SHANGHAI, L.LONGBEACH, DateTime.Now));
            cargo.AssignToRoute(new Itinerary(Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH)));
            Assert.AreEqual(L.LONGBEACH, cargo.CustomsClearancePoint);

            //Cargo destination HAMBURG
            //SHA-LGB-NYC This itinerary does not take
            // the cargo into its CustomsZone, so no clearancePoint.
            cargo.SpecifyNewRoute(new RouteSpecification(L.SHANGHAI, L.HAMBURG, DateTime.Now));
            cargo.AssignToRoute(new Itinerary(Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH),
                Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK)));
            Assert.IsNull(cargo.CustomsClearancePoint);

            //Cargo destination NEWYORK on SHA-LGB-CHI
            //This itinerary does not take the cargo to its destination,
            //but it does enter the CustomsZone, so it has a clearancePoint.
            cargo.SpecifyNewRoute(new RouteSpecification(L.SHANGHAI, L.NEWYORK, DateTime.Now));
            cargo.AssignToRoute(new Itinerary(Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH),
                Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.CHICAGO)));
            Assert.AreEqual(L.LONGBEACH, cargo.CustomsClearancePoint);
        }

        private Cargo shanghaiSeattleChicagoOnPacific2AndContinental3()
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"),
                new RouteSpecification(L.SHANGHAI, L.CHICAGO, DateTime.Parse("2009-12-24")));

            // A cargo with no itinerary is not misdirected
            Assert.IsFalse(cargo.IsMisdirected);

            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(V.pacific2, L.SHANGHAI, L.SEATTLE),
                Leg.DeriveLeg(V.continental3, L.SEATTLE, L.CHICAGO));
            cargo.AssignToRoute(itinerary);
            return cargo;
        }

        private Cargo setUpCargoWithItinerary(Location origin, Location midpoint, Location destination)
        {
            Cargo cargo = new Cargo(new TrackingId("CARGO1"), new RouteSpecification(origin, destination, DateTime.Now));

            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(crazyVoyage, origin, midpoint),
                Leg.DeriveLeg(crazyVoyage, midpoint, destination));

            cargo.AssignToRoute(itinerary);
            return cargo;
        }

        private Cargo populateCargoReceivedStockholm()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);
            cargo.Handled(new HandlingActivity(HandlingActivityType.RECEIVE, L.STOCKHOLM));
            return cargo;
        }

        private Cargo populateCargoClaimedMelbourne()
        {
            Cargo cargo = populateCargoOffMelbourne();

            cargo.Handled(new HandlingActivity(HandlingActivityType.CLAIM, L.MELBOURNE));
            return cargo;
        }

        private Cargo populateCargoOffHongKong()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.Handled(new HandlingActivity(HandlingActivityType.LOAD, L.STOCKHOLM, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.HAMBURG, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.LOAD, L.HAMBURG, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.MELBOURNE, crazyVoyage));
            return cargo;
        }

        private Cargo populateCargoOnHamburg()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.Handled(new HandlingActivity(HandlingActivityType.LOAD, L.STOCKHOLM, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.HAMBURG, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.LOAD, L.HAMBURG, crazyVoyage));
            return cargo;
        }

        private Cargo populateCargoOffMelbourne()
        {
            Cargo cargo = setUpCargoWithItinerary(L.STOCKHOLM, L.HAMBURG, L.MELBOURNE);

            cargo.Handled(new HandlingActivity(HandlingActivityType.LOAD, L.STOCKHOLM, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.HAMBURG, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.LOAD, L.HAMBURG, crazyVoyage));
            cargo.Handled(new HandlingActivity(HandlingActivityType.UNLOAD, L.MELBOURNE, crazyVoyage));

            return cargo;
        }
    }
}