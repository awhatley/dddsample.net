using System;
using System.Collections.Generic;
using System.Linq;

namespace DomainDrivenDelivery.Booking.Api
{
    /// <summary>
    /// DTO for registering and routing a cargo.
    /// </summary>
    [Serializable]
    public sealed class CargoRoutingDTO
    {
        private readonly string trackingId;
        private readonly string origin;
        private readonly string finalDestination;
        private readonly DateTime arrivalDeadline;
        private readonly bool misrouted;
        private readonly IEnumerable<LegDTO> legs;

        public CargoRoutingDTO(string trackingId, string origin, string finalDestination,
                               DateTime arrivalDeadline, bool misrouted, IEnumerable<LegDTO> legs)
        {
            this.trackingId = trackingId;
            this.origin = origin;
            this.finalDestination = finalDestination;
            this.arrivalDeadline = arrivalDeadline;
            this.misrouted = misrouted;
            this.legs = new List<LegDTO>(legs);
        }

        public string getTrackingId()
        {
            return trackingId;
        }

        public string getOrigin()
        {
            return origin;
        }

        public string getFinalDestination()
        {
            return finalDestination;
        }

        public IEnumerable<LegDTO> getLegs()
        {
            return legs.ToList().AsReadOnly();
        }

        public bool isMisrouted()
        {
            return misrouted;
        }

        public bool isRouted()
        {
            return legs.Any();
        }

        public DateTime getArrivalDeadline()
        {
            return arrivalDeadline;
        }
    }
}