using System;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// A carrier movement is a vessel voyage from one location to another.
    /// </summary>
    public class CarrierMovement : ValueObjectSupport<CarrierMovement>
    {
        /// <summary>
        /// Departure location.
        /// </summary>
        public virtual Location DepartureLocation { get; private set; }

        /// <summary>
        /// Arrival location.
        /// </summary>
        public virtual Location ArrivalLocation { get; private set; }

        /// <summary>
        /// Time of departure.
        /// </summary>
        public virtual DateTime DepartureTime { get; private set; }

        /// <summary>
        /// Time of arrival.
        /// </summary>
        public virtual DateTime ArrivalTime { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="departureLocation">location of departure</param>
        /// <param name="arrivalLocation">location of arrival</param>
        /// <param name="departureTime">time of departure</param>
        /// <param name="arrivalTime">time of arrival</param>
        internal CarrierMovement(Location departureLocation,
                        Location arrivalLocation,
                        DateTime departureTime,
                        DateTime arrivalTime)
        {
            Validate.notNull(departureLocation, "Departure location is required");
            Validate.notNull(arrivalLocation, "Arrival location is required");
            Validate.isTrue(arrivalTime > departureTime, "Arrival time must be after departure time");
            Validate.isTrue(!departureLocation.sameAs(arrivalLocation), "Departure location can't be the same as the arrival location");

            DepartureTime = departureTime;
            ArrivalTime = arrivalTime;
            DepartureLocation = departureLocation;
            ArrivalLocation = arrivalLocation;
        }

        /// <summary>
        /// Gets a new CarrierMovement which is a copy of the old one but with a new departure time
        /// </summary>
        /// <param name="newDepartureTime">new departure time</param>
        /// <returns>A new CarrierMovement which is a copy of the old one but with a new departure time</returns>
        protected internal virtual CarrierMovement WithDepartureTime(DateTime newDepartureTime)
        {
            return new CarrierMovement(
              DepartureLocation,
              ArrivalLocation,
              newDepartureTime,
              ArrivalTime
            );
        }

        protected internal CarrierMovement()
        {
        }
    }
}