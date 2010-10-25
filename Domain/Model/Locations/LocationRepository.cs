using System.Collections.Generic;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// Location repository.
    /// </summary>
    public interface LocationRepository
    {
        /// <summary>
        /// Finds a location using given unlocode.
        /// </summary>
        /// <param name="unLocode">UNLocode.</param>
        /// <returns>Location.</returns>
        Location find(UnLocode unLocode);

        /// <summary>
        /// Finds all locations.
        /// </summary>
        /// <returns>All locations.</returns>
        IEnumerable<Location> findAll();
    }
}