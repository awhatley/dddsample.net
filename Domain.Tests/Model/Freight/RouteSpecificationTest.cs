using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Tests.Model.Freight
{
    [TestFixture]
    public class RouteSpecificationTest
    {
        private static Voyage hongKongTokyoNewYork = new Voyage.Builder(new VoyageNumber("V001"), L.HONGKONG)
            .addMovement(L.TOKYO, DateTime.Parse("2009-02-01"), DateTime.Parse("2009-02-05"))
            .addMovement(L.NEWYORK, DateTime.Parse("2009-02-06"), DateTime.Parse("2009-02-10"))
            .addMovement(L.HONGKONG, DateTime.Parse("2009-02-11"), DateTime.Parse("2009-02-14"))
            .build();

        private static Voyage dallasNewYorkChicago =
            new Voyage.Builder(new VoyageNumber("V002"), L.DALLAS)
            .addMovement(L.NEWYORK, DateTime.Parse("2009-02-06"), DateTime.Parse("2009-02-07"))
            .addMovement(L.CHICAGO, DateTime.Parse("2009-02-12"), DateTime.Parse("2009-02-20"))
            .build();

        private static Itinerary itinerary = new Itinerary(
            Leg.deriveLeg(hongKongTokyoNewYork, L.HONGKONG, L.NEWYORK),
            Leg.deriveLeg(dallasNewYorkChicago, L.NEWYORK, L.CHICAGO));

        [Test]
        public void testIsSatisfiedBy_Success()
        {
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG,
                L.CHICAGO,
                DateTime.Parse("2009-03-01"));

            Assert.True(routeSpecification.isSatisfiedBy(itinerary));
        }

        [Test]
        public void testIsSatisfiedBy_WrongOrigin()
        {
            RouteSpecification routeSpecification = new RouteSpecification(L.HANGZOU,
                L.CHICAGO,
                DateTime.Parse("2009-03-01"));

            Assert.False(routeSpecification.isSatisfiedBy(itinerary));
        }

        [Test]
        public void testIsSatisfiedBy_WrongDestination()
        {
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG,
                L.DALLAS,
                DateTime.Parse("2009-03-01"));

            Assert.False(routeSpecification.isSatisfiedBy(itinerary));
        }

        [Test]
        public void testIsSatisfiedBy_MissedDeadline()
        {
            RouteSpecification routeSpecification = new RouteSpecification(L.HONGKONG,
                L.CHICAGO,
                DateTime.Parse("2009-02-15"));

            Assert.False(routeSpecification.isSatisfiedBy(itinerary));
        }

        [Test]
        public void testEquals()
        {
            RouteSpecification HKG_DAL = new RouteSpecification(L.HONGKONG, L.DALLAS, DateTime.Parse("2009-03-01"));
            RouteSpecification HKG_DAL_AGAIN = new RouteSpecification(L.HONGKONG, L.DALLAS, DateTime.Parse("2009-03-01"));
            RouteSpecification SHA_DAL = new RouteSpecification(L.SHANGHAI, L.DALLAS, DateTime.Parse("2009-03-01"));
            RouteSpecification HKG_CHI = new RouteSpecification(L.HONGKONG, L.CHICAGO, DateTime.Parse("2009-03-01"));
            RouteSpecification HKG_DAL_LATERARRIVAL = new RouteSpecification(L.HONGKONG,
                L.DALLAS,
                DateTime.Parse("2009-03-15"));

            Assert.AreEqual(HKG_DAL, HKG_DAL_AGAIN);
            Assert.False(HKG_DAL.Equals(SHA_DAL));
            Assert.False(HKG_DAL.Equals(HKG_CHI));
            Assert.False(HKG_DAL.Equals(HKG_DAL_LATERARRIVAL));
        }

        [Test]
        public void testDeriveWithNewDestination()
        {
            RouteSpecification original = new RouteSpecification(L.HONGKONG, L.DALLAS, DateTime.Parse("2009-03-01"));
            RouteSpecification desired = new RouteSpecification(L.HONGKONG, L.CHICAGO, DateTime.Parse("2009-03-01"));
            Assert.AreEqual(desired, original.withDestination(L.CHICAGO));
        }
    }
}