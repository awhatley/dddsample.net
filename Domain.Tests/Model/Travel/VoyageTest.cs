using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;
using V = DomainDrivenDelivery.Domain.Model.Travel.SampleVoyages;

namespace DomainDrivenDelivery.Domain.Tests.Model.Travel
{
    [TestFixture]
    public class VoyageTest
    {
        [Test]
        public void locations()
        {
            Voyage voyage = V.HONGKONG_TO_NEW_YORK;
            Assert.AreEqual(new[] {L.HONGKONG, L.HANGZOU, L.TOKYO, L.MELBOURNE, L.NEWYORK}, voyage.Locations);
        }
    }
}