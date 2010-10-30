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
            Itinerary itinerary = new Itinerary(Leg.deriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH),
                Leg.deriveLeg(V.continental2, L.LONGBEACH, L.DALLAS));

            LegActivityMatch startMatch = LegActivityMatch.match(Leg.deriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH),
                HandlingActivity.loadOnto(V.pacific1).@in(L.TOKYO),
                itinerary);

            Assert.That(startMatch.handlingActivity(), Is.EqualTo(HandlingActivity.loadOnto(V.pacific1).@in(L.TOKYO)));
            Assert.That(startMatch.leg(), Is.EqualTo(Leg.deriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH)));

            LegActivityMatch endMatch = LegActivityMatch.match(Leg.deriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH),
                HandlingActivity.unloadOff(V.pacific1).@in(L.LONGBEACH),
                itinerary);

            Assert.That(endMatch.handlingActivity(), Is.EqualTo(HandlingActivity.unloadOff(V.pacific1).@in(L.LONGBEACH)));
            Assert.That(endMatch.leg(), Is.EqualTo(Leg.deriveLeg(V.pacific1, L.TOKYO, L.LONGBEACH)));

            LegActivityMatch nextMatch = LegActivityMatch.match(Leg.deriveLeg(V.continental2, L.LONGBEACH, L.DALLAS),
                HandlingActivity.loadOnto(V.continental2).@in(L.LONGBEACH),
                itinerary);

            Assert.That(nextMatch.handlingActivity(),
                Is.EqualTo(HandlingActivity.loadOnto(V.continental2).@in(L.LONGBEACH)));
            Assert.That(nextMatch.leg(), Is.EqualTo(Leg.deriveLeg(V.continental2, L.LONGBEACH, L.DALLAS)));

            Assert.That(startMatch.CompareTo(endMatch), Is.EqualTo(-1));
            Assert.That(endMatch.CompareTo(startMatch), Is.EqualTo(1));
            Assert.That(endMatch.CompareTo(nextMatch), Is.EqualTo(-1));
            Assert.That(nextMatch.CompareTo(endMatch), Is.EqualTo(1));

            Assert.That(startMatch.CompareTo(startMatch), Is.EqualTo(0));
        }
    }
}