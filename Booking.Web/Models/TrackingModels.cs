using System.Collections.Generic;

namespace DomainDrivenDelivery.Booking.Web.Models
{
    public class CargoTrackingViewModel
    {
        public string TrackingId { get; set; }
        public string StatusText { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Eta { get; set; }
        public string NextExpectedActivity { get; set; }
        public bool IsMisdirected { get; set; }
        public IEnumerable<CargoHandlingEventViewModel> Events { get; set; }
    }

    public class CargoHandlingEventViewModel
    {
        public bool IsExpected { get; set; }
        public string Location { get; set; }
        public string Time { get; set; }
        public string Type { get; set; }
        public string VoyageNumber { get; set; }
        public string Description { get; set; }
    }
}