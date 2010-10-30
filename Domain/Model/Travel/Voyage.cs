using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.Entity;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    public class Voyage : EntitySupport<Voyage, VoyageNumber>
    {
        private readonly VoyageNumber _voyageNumber;
        private Schedule _schedule;

        // Null object pattern
        public static readonly Voyage NONE = new Voyage(new VoyageNumber(""), Schedule.EMPTY);

        public Voyage(VoyageNumber voyageNumber, Schedule schedule)
        {
            Validate.notNull(voyageNumber, "Voyage number is required");
            Validate.notNull(schedule, "Schedule is required");

            this._voyageNumber = voyageNumber;
            this._schedule = schedule;
        }

        public override VoyageNumber identity()
        {
            return _voyageNumber;
        }

        /// <summary>
        /// Voyage number.
        /// </summary>
        /// <returns>Voyage number.</returns>
        public VoyageNumber voyageNumber()
        {
            return _voyageNumber;
        }

        /// <summary>
        /// Schedule.
        /// </summary>
        /// <returns>Schedule.</returns>
        public Schedule schedule()
        {
            return _schedule;
        }

        /// <summary>
        /// Reschedules departure from the specified location.
        /// </summary>
        /// <param name="location">location from where the rescheduled departure happens.</param>
        /// <param name="newDepartureTime">new departure time</param>
        public void departureRescheduled(Location location, DateTime newDepartureTime)
        {
            var carrierMovements = new List<CarrierMovement>();

            foreach(CarrierMovement carrierMovement in _schedule.carrierMovements())
            {
                if(carrierMovement.departureLocation().sameAs(location))
                {
                    carrierMovements.Add(carrierMovement.withDepartureTime(newDepartureTime));
                }
                else
                {
                    carrierMovements.Add(carrierMovement);
                }
            }

            this._schedule = new Schedule(carrierMovements);
        }


        public Location arrivalLocationWhenDepartedFrom(Location departureLocation)
        {
            foreach(CarrierMovement carrierMovement in _schedule.carrierMovements())
            {
                if(carrierMovement.departureLocation().sameAs(departureLocation))
                {
                    return carrierMovement.arrivalLocation();
                }
            }

            return Location.NONE;
        }

        public IEnumerable<Location> locations()
        {
            var locations = _schedule.carrierMovements()
                .Select(cm => cm.departureLocation())
                .ToList();

            locations.Add(_schedule.carrierMovements().Last().arrivalLocation());
               
            return locations.AsReadOnly();
        }

        public override string ToString()
        {
            return _voyageNumber.stringValue();
        }

        Voyage()
        {
            // Needed by Hibernate
            _voyageNumber = null;
        }

        /// <summary>
        /// Builder pattern is used for incremental construction
        /// of a Voyage aggregate. This serves as an aggregate factory.
        /// </summary>
        public sealed class Builder
        {
            private readonly List<CarrierMovement> carrierMovements = new List<CarrierMovement>();
            private readonly VoyageNumber voyageNumber;
            private Location currentDepartureLocation;

            public Builder(VoyageNumber voyageNumber, Location initialDepartureLocation)
            {
                Validate.notNull(voyageNumber, "Voyage number is required");
                Validate.notNull(initialDepartureLocation, "Departure location is required");

                this.voyageNumber = voyageNumber;
                this.currentDepartureLocation = initialDepartureLocation;
            }

            public Builder addMovement(Location arrivalLocation, DateTime departureTime, DateTime arrivalTime)
            {
                carrierMovements.Add(new CarrierMovement(currentDepartureLocation, arrivalLocation, departureTime, arrivalTime));
                // Next departure location is the same as this arrival location
                this.currentDepartureLocation = arrivalLocation;
                return this;
            }

            public Voyage build()
            {
                return new Voyage(voyageNumber, new Schedule(carrierMovements));
            }
        }
    }
}