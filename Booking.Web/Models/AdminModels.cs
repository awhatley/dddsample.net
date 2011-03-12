using System;
using System.Collections.Generic;

using DomainDrivenDelivery.Booking.Api;

namespace DomainDrivenDelivery.Booking.Web.Models
{
    public sealed class VoyageDelayedFormModel
    {
        public Dictionary<string, IEnumerable<string>> Departures { get; set; }
        public Dictionary<string, IEnumerable<string>> Arrivals { get; set; }
        public IEnumerable<VoyageDTO> Voyages { get; set; }
    }

    public sealed class PickNewDestinationModel
    {
        public CargoRoutingDTO Cargo { get; set; }
        public IEnumerable<LocationDTO> Locations { get; set; }
    }

    public sealed class SelectItineraryModel
    {
        public CargoRoutingDTO Cargo { get; set; }
        public IEnumerable<RouteCandidateDTO> RouteCandidates { get; set; }
    }

    public sealed class CargoBookingCommand
    {
        public string originUnlocode { get; set; }
        public string destinationUnlocode { get; set; }
        public string arrivalDeadline { get; set; }
    }

    public sealed class RouteAssignmentCommand
    {
        public string trackingId { get; set; }
        public IEnumerable<LegCommand> legs { get; set; }

        public class LegCommand
        {
            public string voyageNumber { get; set; }
            public string fromUnLocode { get; set; }
            public string toUnLocode { get; set; }
            public DateTime fromDate { get; set; }
            public DateTime toDate { get; set; }
        }
    }

    public sealed class VoyageDelayCommand
    {
        public DelayType type { get; set; }
        public string voyageNumber { get; set; }
        public int hours { get; set; }

        public enum DelayType
        {
            DEPT,
            ARR
        }
    }
}