using System;
using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// A voyage schedule.
    /// </summary>
    public class Schedule : ValueObjectSupport<Schedule>
    {
        /// <summary>
        /// Carrier movements.
        /// </summary>
        /// <returns>Carrier movements.</returns>
        public IEnumerable<CarrierMovement> CarrierMovements { get; private set; }

        public static readonly Schedule Empty = new Schedule();

        internal Schedule(IEnumerable<CarrierMovement> carrierMovements)
        {
            Validate.notNull(carrierMovements, "Carrier movements are required");
            Validate.noNullElements(carrierMovements, "There are null elements in the list of carrier movments");
            Validate.notEmpty(carrierMovements, "There must be at least one carrier movement in a schedule");

            CarrierMovements = carrierMovements;
        }

        /// <summary>
        /// Date of departure from this location, or null if it's not part of the voyage.
        /// </summary>
        /// <param name="location">location of departure</param>
        /// <returns>Date of departure from this location, or null if it's not part of the voyage.</returns>
        public DateTime DepartureTimeAt(Location location)
        {
            foreach(var movement in CarrierMovements)
            {
                if(movement.DepartureLocation.sameAs(location))
                {
                    return movement.DepartureTime;
                }
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Date of arrival at this location, or null if it's not part of the voyage.
        /// </summary>
        /// <param name="location">location of arrival</param>
        /// <returns>Date of arrival at this location, or null if it's not part of the voyage.</returns>
        public DateTime ArrivalTimeAt(Location location)
        {
            foreach(var movement in CarrierMovements)
            {
                if(movement.ArrivalLocation.sameAs(location))
                {
                    return movement.ArrivalTime;
                }
            }
            return DateTime.MinValue;
        }

        protected internal Schedule()
        {
            CarrierMovements = new List<CarrierMovement>();
        }
    }
}