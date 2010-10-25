using System;
using System.Collections.Generic;

namespace DomainDrivenDelivery.Booking.Api
{
    [Serializable]
    public class VoyageDTO
    {
        private readonly String voyageNumber;
        private readonly IEnumerable<CarrierMovementDTO> movements;

        public VoyageDTO(String voyageNumber, IEnumerable<CarrierMovementDTO> movements)
        {
            this.voyageNumber = voyageNumber;
            this.movements = movements;
        }

        public String getVoyageNumber()
        {
            return voyageNumber;
        }

        public IEnumerable<CarrierMovementDTO> getMovements()
        {
            return movements;
        }
    }
}