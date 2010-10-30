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
            Leg.deriveLeg(null, null, null);
        }

        [Test]
        public void legThatFollowsPartOfAVoyage()
        {
            Leg chicagoToDallas = Leg.deriveLeg(voyage, L.CHICAGO, L.DALLAS);

            Assert.AreEqual(chicagoToDallas.loadTime(), DateTime.Parse("2008-10-24 21:25"));
            Assert.AreEqual(chicagoToDallas.loadLocation(), L.CHICAGO);
            Assert.AreEqual(chicagoToDallas.unloadTime(), DateTime.Parse("2008-10-25 19:30"));
            Assert.AreEqual(chicagoToDallas.unloadLocation(), L.DALLAS);
        }

        [Test]
        public void legThatFollowsAnEntireVoyage()
        {
            Leg newYorkToDallas = Leg.deriveLeg(voyage, L.NEWYORK, L.DALLAS);

            Assert.AreEqual(newYorkToDallas.loadTime(), DateTime.Parse("2008-10-24 07:00"));
            Assert.AreEqual(newYorkToDallas.loadLocation(), L.NEWYORK);
            Assert.AreEqual(newYorkToDallas.unloadTime(), DateTime.Parse("2008-10-25 19:30"));
            Assert.AreEqual(newYorkToDallas.unloadLocation(), L.DALLAS);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void locationsInWrongOrder()
        {
            Leg.deriveLeg(voyage, L.DALLAS, L.CHICAGO);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void endLocationNotOnVoyage()
        {
            Leg.deriveLeg(voyage, L.CHICAGO, L.HELSINKI);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void startLocationNotOnVoyage()
        {
            Leg.deriveLeg(voyage, L.HONGKONG, L.DALLAS);
        }

        [Test]
        public void matchActivity()
        {
            Leg newYorkToDallas = Leg.deriveLeg(voyage, L.NEWYORK, L.DALLAS);

            Assert.True(newYorkToDallas.matchesActivity(HandlingActivity.loadOnto(voyage).@in(L.NEWYORK)));
            Assert.True(newYorkToDallas.matchesActivity(HandlingActivity.unloadOff(voyage).@in(L.DALLAS)));
            Assert.False(newYorkToDallas.matchesActivity(HandlingActivity.loadOnto(voyage).@in(L.DALLAS)));
            Assert.False(newYorkToDallas.matchesActivity(HandlingActivity.unloadOff(voyage).@in(L.NEWYORK)));
        }

        [Test]
        public void deriveActivities()
        {
            Leg newYorkToDallas = Leg.deriveLeg(voyage, L.NEWYORK, L.DALLAS);

            Assert.That(newYorkToDallas.deriveLoadActivity(),
                Is.EqualTo(HandlingActivity.loadOnto(voyage).@in(L.NEWYORK)));
            Assert.That(newYorkToDallas.deriveUnloadActivity(),
                Is.EqualTo(HandlingActivity.unloadOff(voyage).@in(L.DALLAS)));
        }

        [Test]
        public void intermediateLocations()
        {
            Leg leg = Leg.deriveLeg(SampleVoyages.HONGKONG_TO_NEW_YORK, L.HONGKONG, L.NEWYORK);
            Assert.AreEqual(new[] {L.HANGZOU, L.TOKYO, L.MELBOURNE}.ToList(), leg.intermediateLocations());
        }
    }
}