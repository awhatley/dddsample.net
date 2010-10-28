using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Freight;

namespace DomainDrivenDelivery.Domain.Services
{
    /// <summary>
    /// Routing service.
    /// </summary>
    public interface RoutingService
    {
        /// <summary>
        /// Finds routes that match a specification.
        /// </summary>
        /// <param name="routeSpecification">route specification</param>
        /// <returns>A list of itineraries that satisfy the specification. May be an empty list if no route is found.</returns>
        IEnumerable<Itinerary> fetchRoutesForSpecification(RouteSpecification routeSpecification);
    }
}