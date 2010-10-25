using System;

namespace DomainDrivenDelivery.Booking.Api
{
    [Serializable]
    public class VoyageDelayDTO
    {
        private readonly string voyageNumber;
        private readonly int minutesOfDelay;

        public VoyageDelayDTO(string voyageNumber, int minutesOfDelay)
        {
            this.voyageNumber = voyageNumber;
            this.minutesOfDelay = minutesOfDelay;
        }

        public string getVoyageNumber()
        {
            return voyageNumber;
        }

        public int getMinutesOfDelay()
        {
            return minutesOfDelay;
        }
    }
}