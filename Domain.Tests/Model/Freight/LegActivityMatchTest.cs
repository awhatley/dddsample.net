using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Shared;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;
using V = DomainDrivenDelivery.Domain.Model.Travel.SampleVoyages;

namespace DomainDrivenDelivery.Domain.Tests.Model.Freight
{
    [TestFixture]
    public class LegActivityMatchTest
    {
        [Test]
        public void compareMatches()
        {
            Itinerary itinerary = new Itinerary(Leg.DeriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH),
                Leg.DeriveLeg(V.continental2, L.LONGBEACH, L.DALLAS));

            LegActivityMatch startMatch = LegActivityMatch.Match(Leg.DeriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH),
                HandlingActivity.LoadOnto(V.pacific1).In(L.TOKYO),
                itinerary);

            Assert.That(startMatch.HandlingActivity, Is.EqualTo(HandlingActivity.LoadOnto(V.pacific1).In(L.TOKYO)));
            Assert.That(startMatch.Leg, Is.EqualTo(Leg.DeriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH)));

            LegActivityMatch endMatch = LegActivityMatch.Match(Leg.DeriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH),
                HandlingActivity.UnloadOff(V.pacific1).In(L.LONGBEACH),
                itinerary);

            Assert.That(endMatch.HandlingActivity, Is.EqualTo(HandlingActivity.UnloadOff(V.pacific1).In(L.LONGBEACH)));
            Assert.That(endMatch.Leg, Is.EqualTo(Leg.DeriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH)));

            LegActivityMatch nextMatch = LegActivityMatch.Match(Leg.DeriveLeg(V.continental2, L.LONGBEACH, L.DALLAS),
                HandlingActivity.LoadOnto(V.continental2).In(L.LONGBEACH),
                itinerary);

            Assert.That(nextMatch.HandlingActivity,
                Is.EqualTo(HandlingActivity.LoadOnto(V.continental2).In(L.LONGBEACH)));
            Assert.That(nextMatch.Leg, Is.EqualTo(Leg.DeriveLeg(V.continental2, L.LONGBEACH, L.DALLAS)));

            Assert.That(startMatch.CompareTo(endMatch), Is.EqualTo(-1));
            Assert.That(endMatch.CompareTo(startMatch), Is.EqualTo(1));
            Assert.That(endMatch.CompareTo(nextMatch), Is.EqualTo(-1));
            Assert.That(nextMatch.CompareTo(endMatch), Is.EqualTo(1));

            Assert.That(startMatch.CompareTo(startMatch), Is.EqualTo(0));
        }
    }
}