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
            HandlingActivity activity = HandlingActivity.LoadOnto(V.pacific2).In(L.SEATTLE);
            HandlingActivity copy = activity.Copy();

            Assert.True(activity.sameValueAs(copy));
            Assert.False(activity == copy);
        }
    }
}