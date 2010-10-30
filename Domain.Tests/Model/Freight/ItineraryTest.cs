using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;

namespace DomainDrivenDelivery.Domain.Tests.Model.Freight
{
    [TestFixture]
    public class ItineraryTest
    {
        private Voyage voyage, wrongVoyage, pacific, transcontinental, atlantic;

        [SetUp]
        protected void setUp()
        {
            pacific = new Voyage.Builder(new VoyageNumber("4567"), L.SHANGHAI)
                .addMovement(L.LONGBEACH, new DateTime(1), new DateTime(2))
                .build();

            transcontinental = new Voyage.Builder(new VoyageNumber("4567"), L.LONGBEACH)
                .addMovement(L.CHICAGO, new DateTime(1), new DateTime(2))
                .addMovement(L.NEWYORK, new DateTime(3), new DateTime(4))
                .build();

            atlantic = new Voyage.Builder(new VoyageNumber("4556"), L.NEWYORK)
                .addMovement(L.ROTTERDAM, new DateTime(1), new DateTime(2))
                .addMovement(L.GOTHENBURG, new DateTime(3), new DateTime(4))
                .build();

            voyage = new Voyage.Builder(new VoyageNumber("0123"), L.SHANGHAI)
                .addMovement(L.ROTTERDAM, new DateTime(1), new DateTime(2))
                .addMovement(L.GOTHENBURG, new DateTime(3), new DateTime(4))
                .build();

            wrongVoyage = new Voyage.Builder(new VoyageNumber("666"), L.NEWYORK)
                .addMovement(L.STOCKHOLM, new DateTime(1), new DateTime(2))
                .addMovement(L.HELSINKI, new DateTime(3), new DateTime(4))
                .build();
         }

        [Test]
        public void testIfCargoIsOnTrack()
        {
            Itinerary itinerary = new Itinerary(Leg.deriveLeg(voyage, L.SHANGHAI, L.ROTTERDAM),
                Leg.deriveLeg(voyage, L.ROTTERDAM, L.GOTHENBURG));

            // HandlingActivity.Load(cargo, HandlingActivityType.RECEIVE, L.SHANGHAI, toDate("2009-05-03"))
            //Happy path
            HandlingActivity receiveShanghai = new HandlingActivity(HandlingActivityType.RECEIVE, L.SHANGHAI);
            Assert.IsTrue(itinerary.isExpectedActivity(receiveShanghai));

            HandlingActivity loadShanghai = new HandlingActivity(HandlingActivityType.LOAD, L.SHANGHAI, voyage);
            Assert.IsTrue(itinerary.isExpectedActivity(loadShanghai));

            HandlingActivity unloadRotterdam = new HandlingActivity(HandlingActivityType.UNLOAD, L.ROTTERDAM, voyage);
            Assert.IsTrue(itinerary.isExpectedActivity(unloadRotterdam));

            HandlingActivity loadRotterdam = new HandlingActivity(HandlingActivityType.LOAD, L.ROTTERDAM, voyage);
            Assert.IsTrue(itinerary.isExpectedActivity(loadRotterdam));

            HandlingActivity unloadGothenburg = new HandlingActivity(HandlingActivityType.UNLOAD, L.GOTHENBURG, voyage);
            Assert.IsTrue(itinerary.isExpectedActivity(unloadGothenburg));

            HandlingActivity claimGothenburg = new HandlingActivity(HandlingActivityType.CLAIM, L.GOTHENBURG);
            Assert.IsTrue(itinerary.isExpectedActivity(claimGothenburg));

            //TODO Customs event can only be interpreted properly by knowing the destination of the cargo.
            // This can be inferred from the Itinerary, but it isn't definitive. So, do we answer based on
            // the end of the itinerary (even though this would probably not be used in the app) or do we
            // ignore this at itinerary level somehow and leave it purely as a cargo responsibility.
            // (See customsClearancePoint tests in CargoTest)
            //    HandlingActivity customsGothenburg = new HandlingActivity(CUSTOMS, L.GOTHENBURG);
            //    Assert.IsTrue(itinerary.isExpectedActivity(customsGothenburg));

            //Received at the wrong location
            HandlingActivity receiveHangzou = new HandlingActivity(HandlingActivityType.RECEIVE, L.HANGZOU);
            Assert.IsFalse(itinerary.isExpectedActivity(receiveHangzou));

            //Loaded to onto the wrong ship, correct location
            HandlingActivity loadRotterdam666 = new HandlingActivity(HandlingActivityType.LOAD, L.ROTTERDAM, wrongVoyage);
            Assert.IsFalse(itinerary.isExpectedActivity(loadRotterdam666));

            //Unloaded from the wrong ship in the wrong location
            HandlingActivity unloadHelsinki = new HandlingActivity(HandlingActivityType.UNLOAD, L.HELSINKI, wrongVoyage);
            Assert.IsFalse(itinerary.isExpectedActivity(unloadHelsinki));

            HandlingActivity claimRotterdam = new HandlingActivity(HandlingActivityType.CLAIM, L.ROTTERDAM);
            Assert.IsFalse(itinerary.isExpectedActivity(claimRotterdam));
        }

        [Test]
        public void testMatchingLeg()
        {
            Leg shanghaiToRotterdam = Leg.deriveLeg(voyage, L.SHANGHAI, L.ROTTERDAM);
            Leg rotterdamToGothenburg = Leg.deriveLeg(voyage, L.ROTTERDAM, L.GOTHENBURG);
            Itinerary itinerary = new Itinerary(shanghaiToRotterdam, rotterdamToGothenburg);

            Assert.That(itinerary.matchLeg(HandlingActivity.receiveIn(L.SHANGHAI)).leg(),
                Is.EqualTo(shanghaiToRotterdam));
            Assert.That(itinerary.matchLeg(HandlingActivity.loadOnto(voyage).@in(L.SHANGHAI)).leg(),
                Is.EqualTo(shanghaiToRotterdam));
            Assert.That(itinerary.matchLeg(HandlingActivity.unloadOff(voyage).@in(L.ROTTERDAM)).leg(),
                Is.EqualTo(shanghaiToRotterdam));
            Assert.That(itinerary.matchLeg(HandlingActivity.claimIn(L.GOTHENBURG)).leg(),
                Is.EqualTo(rotterdamToGothenburg));

            Assert.IsNull(itinerary.matchLeg(HandlingActivity.loadOnto(wrongVoyage).@in(L.SHANGHAI)).leg());
            Assert.IsNull(itinerary.matchLeg(HandlingActivity.loadOnto(wrongVoyage).@in(L.NEWYORK)).leg());

            Assert.IsNull(itinerary.matchLeg(HandlingActivity.unloadOff(wrongVoyage).@in(L.ROTTERDAM)).leg());
            Assert.IsNull(itinerary.matchLeg(HandlingActivity.unloadOff(wrongVoyage).@in(L.NEWYORK)).leg());

            Assert.IsNull(itinerary.matchLeg(HandlingActivity.receiveIn(L.NEWYORK)).leg());
            Assert.IsNull(itinerary.matchLeg(HandlingActivity.claimIn(L.NEWYORK)).leg());
        }

        [Test]
        public void testNextLeg()
        {
            Leg shanghaiToLongBeach = Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH);
            Leg longBeachToNewYork = Leg.deriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK);
            Leg newYorkToRotterdam = Leg.deriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM);

            Itinerary itinerary = new Itinerary(shanghaiToLongBeach, longBeachToNewYork, newYorkToRotterdam);

            Assert.That(itinerary.nextLeg(shanghaiToLongBeach), Is.EqualTo(longBeachToNewYork));
            Assert.That(itinerary.nextLeg(longBeachToNewYork), Is.EqualTo(newYorkToRotterdam));
            Assert.IsNull(itinerary.nextLeg(newYorkToRotterdam));
        }

        [Test]
        public void testLatestLeg()
        {
            Leg shanghaiToLongBeach = Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH);
            Leg longBeachToNewYork = Leg.deriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK);
            Leg newYorkToRotterdam = Leg.deriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM);

            Itinerary itinerary = new Itinerary(shanghaiToLongBeach, longBeachToNewYork, newYorkToRotterdam);

            HandlingActivity notOnRoute = HandlingActivity.loadOnto(pacific).@in(L.STOCKHOLM);
            HandlingActivity loadInShanghai = HandlingActivity.loadOnto(pacific).@in(L.SHANGHAI);
            HandlingActivity unloadInLongbeach = HandlingActivity.unloadOff(pacific).@in(L.LONGBEACH);

            Assert.That(itinerary.strictlyPriorOf(loadInShanghai, unloadInLongbeach), Is.EqualTo(loadInShanghai));
            Assert.That(itinerary.strictlyPriorOf(unloadInLongbeach, loadInShanghai), Is.EqualTo(loadInShanghai));

            Assert.That(itinerary.strictlyPriorOf(unloadInLongbeach, notOnRoute), Is.EqualTo(unloadInLongbeach));
            Assert.That(itinerary.strictlyPriorOf(notOnRoute, loadInShanghai), Is.EqualTo(loadInShanghai));

            Assert.IsNull(itinerary.strictlyPriorOf(unloadInLongbeach, unloadInLongbeach));
        }

        [Test]
        public void testTruncatedAfter()
        {
            Leg shanghaiToLongBeach = Leg.deriveLeg(pacific, L.SHANGHAI, L.LONGBEACH);
            Leg longBeachToNewYork = Leg.deriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK);
            Leg newYorkToRotterdam = Leg.deriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM);

            Itinerary itinerary = new Itinerary(shanghaiToLongBeach, longBeachToNewYork, newYorkToRotterdam);

            Itinerary toNewYork = itinerary.truncatedAfter(L.NEWYORK);
            Assert.AreEqual(new[] {shanghaiToLongBeach, longBeachToNewYork}.ToList(), toNewYork.legs());

            Itinerary toChicago = itinerary.truncatedAfter(L.CHICAGO);
            Assert.AreEqual(
                new[] {shanghaiToLongBeach, Leg.deriveLeg(transcontinental, L.LONGBEACH, L.CHICAGO)}.ToList(),
                toChicago.legs());

            Itinerary toRotterdam = itinerary.truncatedAfter(L.ROTTERDAM);
            Assert.AreEqual(
                new[] {shanghaiToLongBeach, longBeachToNewYork, Leg.deriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM)}.ToList(),
                toRotterdam.legs());
        }

        [Test]
        public void testCreateItinerary()
        {
            try
            {
                new Itinerary(new List<Leg>());
                Assert.Fail("An empty itinerary is not OK");
            }
            catch(ArgumentException)
            {
                //Expected
            }

            try
            {
                new Itinerary((List<Leg>) null);
                Assert.Fail("Null itinerary is not OK");
            }
            catch(ArgumentException)
            {
                //Expected
            }
        }
    }
}