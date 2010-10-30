using System;

using DomainDrivenDelivery.Domain.Model.Freight;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Model.Freight
{
    [TestFixture]
    public class TrackingIdTest
    {
        [Test]
        public void testConstructor()
        {
            try
            {
                new TrackingId(null);
                Assert.Fail("Should not accept null constructor arguments");
            }
            catch(ArgumentNullException)
            {
            }
        }
    }
}