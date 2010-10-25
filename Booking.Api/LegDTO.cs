using System;

namespace DomainDrivenDelivery.Booking.Api
{
    /// <summary>
    /// DTO for a leg in an itinerary.
    /// </summary>
    [Serializable]
    public sealed class LegDTO
    {
        private readonly string voyageNumber;
        private readonly string from;
        private readonly string to;
        private readonly DateTime loadTime;
        private readonly DateTime unloadTime;

        public LegDTO(string voyageNumber, string from, string to, DateTime loadTime, DateTime unloadTime)
        {
            this.voyageNumber = voyageNumber;
            this.from = from;
            this.to = to;
            this.loadTime = loadTime;
            this.unloadTime = unloadTime;
        }

        public string getVoyageNumber()
        {
            return voyageNumber;
        }

        public string getFrom()
        {
            return from;
        }

        public string getTo()
        {
            return to;
        }

        public DateTime getLoadTime()
        {
            return loadTime;
        }

        public DateTime getUnloadTime()
        {
            return unloadTime;
        }
    }
}