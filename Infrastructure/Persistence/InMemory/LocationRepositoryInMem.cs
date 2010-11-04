using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Locations;

namespace DomainDrivenDelivery.Infrastructure.Persistence.InMemory
{
    public class LocationRepositoryInMem : LocationRepository
    {
        public Location find(UnLocode unLocode)
        {
            foreach(Location location in SampleLocations.getAll())
            {
                if(location.UnLocode.Equals(unLocode))
                {
                    return location;
                }
            }
            return null;
        }

        public IEnumerable<Location> findAll()
        {
            return SampleLocations.getAll();
        }
    }
}