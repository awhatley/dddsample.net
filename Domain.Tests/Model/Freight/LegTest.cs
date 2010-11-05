using System;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Tests.Model.Freight
{
    [TestFixture]
    public class LegTest
    {
        private readonly Voyage voyage = SampleVoyages.NEW_YORK_TO_DALLAS;

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void testConstructor()
        {
            Leg.DeriveLeg(null, null, null);
        }

        [Test]
        public void legThatFollowsPartOfAVoyage()
        {
            Leg chicagoToDallas = Leg.DeriveLeg(voyage, L.CHICAGO, L.DALLAS);

            Assert.AreEqual(chicagoToDallas.LoadTime, DateTime.Parse("2008-10-24 21:25"));
            Assert.AreEqual(chicagoToDallas.LoadLocation, L.CHICAGO);
            Assert.AreEqual(chicagoToDallas.UnloadTime, DateTime.Parse("2008-10-25 19:30"));
            Assert.AreEqual(chicagoToDallas.UnloadLocation, L.DALLAS);
        }

        [Test]
        public void legThatFollowsAnEntireVoyage()
        {
            Leg newYorkToDallas = Leg.DeriveLeg(voyage, L.NEWYORK, L.DALLAS);

            Assert.AreEqual(newYorkToDallas.LoadTime, DateTime.Parse("2008-10-24 07:00"));
            Assert.AreEqual(newYorkToDallas.LoadLocation, L.NEWYORK);
            Assert.AreEqual(newYorkToDallas.UnloadTime, DateTime.Parse("2008-10-25 19:30"));
            Assert.AreEqual(newYorkToDallas.UnloadLocation, L.DALLAS);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void locationsInWrongOrder()
        {
            Leg.DeriveLeg(voyage, L.DALLAS, L.CHICAGO);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void endLocationNotOnVoyage()
        {
            Leg.DeriveLeg(voyage, L.CHICAGO, L.HELSINKI);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void startLocationNotOnVoyage()
        {
            Leg.DeriveLeg(voyage, L.HONGKONG, L.DALLAS);
        }

        [Test]
        public void matchActivity()
        {
            Leg newYorkToDallas = Leg.DeriveLeg(voyage, L.NEWYORK, L.DALLAS);

            Assert.True(newYorkToDallas.MatchesActivity(HandlingActivity.LoadOnto(voyage).In(L.NEWYORK)));
            Assert.True(newYorkToDallas.MatchesActivity(HandlingActivity.UnloadOff(voyage).In(L.DALLAS)));
            Assert.False(newYorkToDallas.MatchesActivity(HandlingActivity.LoadOnto(voyage).In(L.DALLAS)));
            Assert.False(newYorkToDallas.MatchesActivity(HandlingActivity.UnloadOff(voyage).In(L.NEWYORK)));
        }

        [Test]
        public void deriveActivities()
        {
            Leg newYorkToDallas = Leg.DeriveLeg(voyage, L.NEWYORK, L.DALLAS);

            Assert.That(newYorkToDallas.DeriveLoadActivity(),
                Is.EqualTo(HandlingActivity.LoadOnto(voyage).In(L.NEWYORK)));
            Assert.That(newYorkToDallas.DeriveUnloadActivity(),
                Is.EqualTo(HandlingActivity.UnloadOff(voyage).In(L.DALLAS)));
        }

        [Test]
        public void intermediateLocations()
        {
            Leg leg = Leg.DeriveLeg(SampleVoyages.HONGKONG_TO_NEW_YORK, L.HONGKONG, L.NEWYORK);
            Assert.AreEqual(new[] {L.HANGZOU, L.TOKYO, L.MELBOURNE}.ToList(), leg.IntermediateLocations);
        }
    }
}