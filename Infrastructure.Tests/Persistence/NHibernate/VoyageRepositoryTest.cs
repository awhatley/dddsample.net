using DomainDrivenDelivery.Domain.Model.Travel;

using NUnit.Framework;

namespace DomainDrivenDelivery.Infrastructure.Tests.Persistence.NHibernate
{
    [TestFixture]
    public class VoyageRepositoryTest : AbstractRepositoryTest
    {
        public VoyageRepository voyageRepository { get; set; }

        [Test]
        public void testFind()
        {
            Voyage voyage = voyageRepository.find(new VoyageNumber("0101"));
            Assert.NotNull(voyage);
            Assert.AreEqual("0101", voyage.VoyageNumber.Value);

            /* TODO adapt
             * assertEquals(STOCKHOLM, carrierMovement.departureLocation());
             * assertEquals(HELSINKI, carrierMovement.arrivalLocation());
             * assertEquals(DateTestUtil.toDate("2007-09-23", "02:00"), carrierMovement.departureTime());
             * assertEquals(DateTestUtil.toDate("2007-09-23", "03:00"), carrierMovement.arrivalTime());
             * */
        }
    }
}