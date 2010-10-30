using DomainDrivenDelivery.Domain.Model.Shared;

using NUnit.Framework;

using L = DomainDrivenDelivery.Domain.Model.Locations.SampleLocations;
using V = DomainDrivenDelivery.Domain.Model.Travel.SampleVoyages;

namespace DomainDrivenDelivery.Domain.Tests.Model.Shared
{
    [TestFixture]
    public class HandlingActivityTest
    {
        [Test]
        public void copy()
        {
            HandlingActivity activity = HandlingActivity.loadOnto(V.pacific2).@in(L.SEATTLE);
            HandlingActivity copy = activity.copy();

            Assert.True(activity.sameValueAs(copy));
            Assert.False(activity == copy);
        }
    }
}