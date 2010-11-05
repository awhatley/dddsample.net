using System.Linq;

using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

namespace DomainDrivenDelivery.Domain.Tests.Model.Travel
{
    [TestFixture]
    public class ScheduleTest
    {
        [Test]
        public void testEmpty()
        {
            Assert.True(!Schedule.Empty.CarrierMovements.Any());
        }

        [Test]
        public void testSameValueAs()
        {
            //TODO: Test goes here...
        }

        [Test]
        public void testCopy()
        {
            //TODO: Test goes here...
        }

        [Test]
        public void testEquals()
        {
            //TODO: Test goes here...
        }

        [Test]
        public void testHashCode()
        {
            //TODO: Test goes here...
        }
    }
}