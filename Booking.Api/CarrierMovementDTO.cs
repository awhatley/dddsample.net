using System;

namespace DomainDrivenDelivery.Booking.Api
{
    [Serializable]
    public class CarrierMovementDTO
    {
        private readonly LocationDTO departureLocation;
        private readonly LocationDTO arrivalLocation;

        public CarrierMovementDTO(LocationDTO departureLocation, LocationDTO arrivalLocation)
        {
            this.departureLocation = departureLocation;
            this.arrivalLocation = arrivalLocation;
        }

        public LocationDTO getDepartureLocation()
        {
            return departureLocation;
        }

        public LocationDTO getArrivalLocation()
        {
            return arrivalLocation;
        }
    }
}