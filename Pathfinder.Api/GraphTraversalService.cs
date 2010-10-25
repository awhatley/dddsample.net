using System.Collections;
using System.Collections.Generic;

namespace DomainDrivenDelivery.Pathfinder.Api
{
    /// <summary>
    /// Part of the external graph traversal API exposed by the routing team
    /// and used by us (booking and tracking team).
    /// </summary>
    public interface GraphTraversalService
    {
        /// <summary>
        /// Finds the shortest path from one location to another with limitations.
        /// </summary>
        /// <param name="originUnLocode">origin UN Locode</param>
        /// <param name="destinationUnLocode">destination UN Locode</param>
        /// <param name="limitations">restrictions on the path selection, as key-value according to some API specification</param>
        /// <returns>A list of transit paths</returns>
        IEnumerable<TransitPath> findShortestPath(string originUnLocode, 
                                                  string destinationUnLocode,
                                                  Hashtable limitations);
    }
}