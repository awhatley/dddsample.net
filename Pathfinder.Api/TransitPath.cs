using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainDrivenDelivery.Pathfinder.Api
{
    [Serializable]
    public sealed class TransitPath
    {
        private readonly IEnumerable<TransitEdge> transitEdges;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transitEdges">The legs for this itinerary.</param>
        public TransitPath(IEnumerable<TransitEdge> transitEdges)
        {
            this.transitEdges = transitEdges;
        }

        /// <summary>
        /// An unmodifiable list DTOs.
        /// </summary>
        /// <returns>An unmodifiable list DTOs.</returns>
        public IEnumerable<TransitEdge> getTransitEdges()
        {
            return transitEdges.ToList().AsReadOnly();
        }
    }
}