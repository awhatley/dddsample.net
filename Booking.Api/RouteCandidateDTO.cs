using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainDrivenDelivery.Booking.Api
{
    /// <summary>
    /// DTO for presenting and selecting an itinerary from a collection of candidates.
    /// </summary>
    [Serializable]
    public sealed class RouteCandidateDTO
    {
        private readonly IEnumerable<LegDTO> legs;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="legs">The legs for this itinerary.</param>
        public RouteCandidateDTO(IEnumerable<LegDTO> legs)
        {
            this.legs = new List<LegDTO>(legs);
        }

        /// <summary>
        /// An unmodifiable list DTOs.
        /// </summary>
        /// <returns>An unmodifiable list DTOs.</returns>
        public IEnumerable<LegDTO> getLegs()
        {
            return legs.ToList().AsReadOnly();
        }
    }
}