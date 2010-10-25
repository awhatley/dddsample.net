using System;

namespace DomainDrivenDelivery.Pathfinder.Api
{
    /// <summary>
    /// Represents an edge in a path through a graph,
    /// describing the route of a cargo.
    /// </summary>
    [Serializable]
    public sealed class TransitEdge
    {
        private readonly String voyageNumber;
        private readonly String fromUnLocode;
        private readonly String toUnLocode;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="voyageNumber">voyage number</param>
        /// <param name="fromUnLocode">UN Locode of start location</param>
        /// <param name="toUnLocode">UN Locode of end location</param>
        public TransitEdge(String voyageNumber,
                           String fromUnLocode,
                           String toUnLocode)
        {
            this.voyageNumber = voyageNumber;
            this.fromUnLocode = fromUnLocode;
            this.toUnLocode = toUnLocode;
        }

        public String getVoyageNumber()
        {
            return voyageNumber;
        }

        public String getFromUnLocode()
        {
            return fromUnLocode;
        }

        public String getToUnLocode()
        {
            return toUnLocode;
        }
    }
}