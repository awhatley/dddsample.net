using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// A voyage schedule.
    /// </summary>
    public class Schedule : ValueObjectSupport<Schedule>
    {
        private readonly IEnumerable<CarrierMovement> _carrierMovements;

        public static readonly Schedule EMPTY = new Schedule();

        internal Schedule(IEnumerable<CarrierMovement> carrierMovements)
        {
            Validate.notNull(carrierMovements, "Carrier movements are required");
            Validate.noNullElements(carrierMovements, "There are null elements in the list of carrier movments");
            Validate.notEmpty(carrierMovements, "There must be at least one carrier movement in a schedule");

            this._carrierMovements = carrierMovements;
        }

        /// <summary>
        /// Carrier movements.
        /// </summary>
        /// <returns>Carrier movements.</returns>
        public IEnumerable<CarrierMovement> carrierMovements()
        {
            return _carrierMovements.ToList().AsReadOnly();
        }

        /// <summary>
        /// Date of departure from this location, or null if it's not part of the voyage.
        /// </summary>
        /// <param name="location">location of departure</param>
        /// <returns>Date of departure from this location, or null if it's not part of the voyage.</returns>
        public DateTime departureTimeAt(Location location)
        {
            foreach(CarrierMovement movement in _carrierMovements)
            {
                if(movement.departureLocation().sameAs(location))
                {
                    return movement.departureTime();
                }
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Date of arrival at this location, or null if it's not part of the voyage.
        /// </summary>
        /// <param name="location">location of arrival</param>
        /// <returns>Date of arrival at this location, or null if it's not part of the voyage.</returns>
        public DateTime arrivalTimeAt(Location location)
        {
            foreach(CarrierMovement movement in _carrierMovements)
            {
                if(movement.arrivalLocation().sameAs(location))
                {
                    return movement.arrivalTime();
                }
            }
            return DateTime.MinValue;
        }

        Schedule()
        {
            // Needed by Hibernate
            _carrierMovements = new List<CarrierMovement>();
        }
    }
}