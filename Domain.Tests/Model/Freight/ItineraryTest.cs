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
            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(voyage, L.SHANGHAI, L.ROTTERDAM),
                Leg.DeriveLeg(voyage, L.ROTTERDAM, L.GOTHENBURG));

            // HandlingActivity.Load(cargo, HandlingActivityType.RECEIVE, L.SHANGHAI, toDate("2009-05-03"))
            //Happy path
            HandlingActivity receiveShanghai = new HandlingActivity(HandlingActivityType.RECEIVE, L.SHANGHAI);
            Assert.IsTrue(itinerary.IsExpectedActivity(receiveShanghai));

            HandlingActivity loadShanghai = new HandlingActivity(HandlingActivityType.LOAD, L.SHANGHAI, voyage);
            Assert.IsTrue(itinerary.IsExpectedActivity(loadShanghai));

            HandlingActivity unloadRotterdam = new HandlingActivity(HandlingActivityType.UNLOAD, L.ROTTERDAM, voyage);
            Assert.IsTrue(itinerary.IsExpectedActivity(unloadRotterdam));

            HandlingActivity loadRotterdam = new HandlingActivity(HandlingActivityType.LOAD, L.ROTTERDAM, voyage);
            Assert.IsTrue(itinerary.IsExpectedActivity(loadRotterdam));

            HandlingActivity unloadGothenburg = new HandlingActivity(HandlingActivityType.UNLOAD, L.GOTHENBURG, voyage);
            Assert.IsTrue(itinerary.IsExpectedActivity(unloadGothenburg));

            HandlingActivity claimGothenburg = new HandlingActivity(HandlingActivityType.CLAIM, L.GOTHENBURG);
            Assert.IsTrue(itinerary.IsExpectedActivity(claimGothenburg));

            //TODO Customs event can only be interpreted properly by knowing the destination of the cargo.
            // This can be inferred from the Itinerary, but it isn't definitive. So, do we answer based on
            // the end of the itinerary (even though this would probably not be used in the app) or do we
            // ignore this at itinerary level somehow and leave it purely as a cargo responsibility.
            // (See customsClearancePoint tests in CargoTest)
            //    HandlingActivity customsGothenburg = new HandlingActivity(CUSTOMS, L.GOTHENBURG);
            //    Assert.IsTrue(itinerary.isExpectedActivity(customsGothenburg));

            //Received at the wrong location
            HandlingActivity receiveHangzou = new HandlingActivity(HandlingActivityType.RECEIVE, L.HANGZOU);
            Assert.IsFalse(itinerary.IsExpectedActivity(receiveHangzou));

            //Loaded to onto the wrong ship, correct location
            HandlingActivity loadRotterdam666 = new HandlingActivity(HandlingActivityType.LOAD, L.ROTTERDAM, wrongVoyage);
            Assert.IsFalse(itinerary.IsExpectedActivity(loadRotterdam666));

            //Unloaded from the wrong ship in the wrong location
            HandlingActivity unloadHelsinki = new HandlingActivity(HandlingActivityType.UNLOAD, L.HELSINKI, wrongVoyage);
            Assert.IsFalse(itinerary.IsExpectedActivity(unloadHelsinki));

            HandlingActivity claimRotterdam = new HandlingActivity(HandlingActivityType.CLAIM, L.ROTTERDAM);
            Assert.IsFalse(itinerary.IsExpectedActivity(claimRotterdam));
        }

        [Test]
        public void testMatchingLeg()
        {
            Leg shanghaiToRotterdam = Leg.DeriveLeg(voyage, L.SHANGHAI, L.ROTTERDAM);
            Leg rotterdamToGothenburg = Leg.DeriveLeg(voyage, L.ROTTERDAM, L.GOTHENBURG);
            Itinerary itinerary = new Itinerary(shanghaiToRotterdam, rotterdamToGothenburg);

            Assert.That(itinerary.MatchLeg(HandlingActivity.ReceiveIn(L.SHANGHAI)).Leg,
                Is.EqualTo(shanghaiToRotterdam));
            Assert.That(itinerary.MatchLeg(HandlingActivity.LoadOnto(voyage).In(L.SHANGHAI)).Leg,
                Is.EqualTo(shanghaiToRotterdam));
            Assert.That(itinerary.MatchLeg(HandlingActivity.UnloadOff(voyage).In(L.ROTTERDAM)).Leg,
                Is.EqualTo(shanghaiToRotterdam));
            Assert.That(itinerary.MatchLeg(HandlingActivity.ClaimIn(L.GOTHENBURG)).Leg,
                Is.EqualTo(rotterdamToGothenburg));

            Assert.IsNull(itinerary.MatchLeg(HandlingActivity.LoadOnto(wrongVoyage).In(L.SHANGHAI)).Leg);
            Assert.IsNull(itinerary.MatchLeg(HandlingActivity.LoadOnto(wrongVoyage).In(L.NEWYORK)).Leg);

            Assert.IsNull(itinerary.MatchLeg(HandlingActivity.UnloadOff(wrongVoyage).In(L.ROTTERDAM)).Leg);
            Assert.IsNull(itinerary.MatchLeg(HandlingActivity.UnloadOff(wrongVoyage).In(L.NEWYORK)).Leg);

            Assert.IsNull(itinerary.MatchLeg(HandlingActivity.ReceiveIn(L.NEWYORK)).Leg);
            Assert.IsNull(itinerary.MatchLeg(HandlingActivity.ClaimIn(L.NEWYORK)).Leg);
        }

        [Test]
        public void testNextLeg()
        {
            Leg shanghaiToLongBeach = Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH);
            Leg longBeachToNewYork = Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK);
            Leg newYorkToRotterdam = Leg.DeriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM);

            Itinerary itinerary = new Itinerary(shanghaiToLongBeach, longBeachToNewYork, newYorkToRotterdam);

            Assert.That(itinerary.NextLeg(shanghaiToLongBeach), Is.EqualTo(longBeachToNewYork));
            Assert.That(itinerary.NextLeg(longBeachToNewYork), Is.EqualTo(newYorkToRotterdam));
            Assert.IsNull(itinerary.NextLeg(newYorkToRotterdam));
        }

        [Test]
        public void testLatestLeg()
        {
            Leg shanghaiToLongBeach = Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH);
            Leg longBeachToNewYork = Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK);
            Leg newYorkToRotterdam = Leg.DeriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM);

            Itinerary itinerary = new Itinerary(shanghaiToLongBeach, longBeachToNewYork, newYorkToRotterdam);

            HandlingActivity notOnRoute = HandlingActivity.LoadOnto(pacific).In(L.STOCKHOLM);
            HandlingActivity loadInShanghai = HandlingActivity.LoadOnto(pacific).In(L.SHANGHAI);
            HandlingActivity unloadInLongbeach = HandlingActivity.UnloadOff(pacific).In(L.LONGBEACH);

            Assert.That(itinerary.StrictlyPriorOf(loadInShanghai, unloadInLongbeach), Is.EqualTo(loadInShanghai));
            Assert.That(itinerary.StrictlyPriorOf(unloadInLongbeach, loadInShanghai), Is.EqualTo(loadInShanghai));

            Assert.That(itinerary.StrictlyPriorOf(unloadInLongbeach, notOnRoute), Is.EqualTo(unloadInLongbeach));
            Assert.That(itinerary.StrictlyPriorOf(notOnRoute, loadInShanghai), Is.EqualTo(loadInShanghai));

            Assert.IsNull(itinerary.StrictlyPriorOf(unloadInLongbeach, unloadInLongbeach));
        }

        [Test]
        public void testTruncatedAfter()
        {
            Leg shanghaiToLongBeach = Leg.DeriveLeg(pacific, L.SHANGHAI, L.LONGBEACH);
            Leg longBeachToNewYork = Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.NEWYORK);
            Leg newYorkToRotterdam = Leg.DeriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM);

            Itinerary itinerary = new Itinerary(shanghaiToLongBeach, longBeachToNewYork, newYorkToRotterdam);

            Itinerary toNewYork = itinerary.TruncatedAfter(L.NEWYORK);
            Assert.AreEqual(new[] {shanghaiToLongBeach, longBeachToNewYork}.ToList(), toNewYork.Legs);

            Itinerary toChicago = itinerary.TruncatedAfter(L.CHICAGO);
            Assert.AreEqual(
                new[] {shanghaiToLongBeach, Leg.DeriveLeg(transcontinental, L.LONGBEACH, L.CHICAGO)}.ToList(),
                toChicago.Legs);

            Itinerary toRotterdam = itinerary.TruncatedAfter(L.ROTTERDAM);
            Assert.AreEqual(
                new[] {shanghaiToLongBeach, longBeachToNewYork, Leg.DeriveLeg(atlantic, L.NEWYORK, L.ROTTERDAM)}.ToList(),
                toRotterdam.Legs);
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