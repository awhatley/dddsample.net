using System;

namespace DomainDrivenDelivery.Booking.Api
{
    /// <summary>
    /// Location DTO
    /// </summary>
    [Serializable]
    public class LocationDTO
    {
        private readonly string unLocode;
        private readonly string name;

        public LocationDTO(string unLocode, string name)
        {
            this.unLocode = unLocode;
            this.name = name;
        }

        public string getUnLocode()
        {
            return unLocode;
        }

        public string getName()
        {
            return name;
        }
    }
}