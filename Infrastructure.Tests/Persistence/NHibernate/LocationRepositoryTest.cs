using System.Linq;

using DomainDrivenDelivery.Domain.Model.Locations;

using NUnit.Framework;

namespace DomainDrivenDelivery.Infrastructure.Tests.Persistence.NHibernate
{
    [TestFixture]
    public class LocationRepositoryTest : AbstractRepositoryTest
    {
        public LocationRepository locationRepository { get; set; }

        [Test]
        public void testFind()
        {
            var melbourne = new UnLocode("AUMEL");
            var location = locationRepository.find(melbourne);

            Assert.NotNull(location);
            Assert.AreEqual(melbourne, location.UnLocode);

            Assert.Null(locationRepository.find(new UnLocode("NOLOC")));
        }

        [Test]
        public void testFindAll()
        {
            var allLocations = locationRepository.findAll();
            
            Assert.NotNull(allLocations);
            Assert.AreEqual(7, allLocations.Count());
        }
    }
}