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

        public override VoyageNumber Identity
        {
            get { return _voyageNumber; }
        }

        /// <summary>
        /// Voyage number.
        /// </summary>
        public virtual VoyageNumber VoyageNumber
        {
            get { return _voyageNumber; }
        }

        /// <summary>
        /// Schedule.
        /// </summary>
        public virtual Schedule Schedule
        {
            get { return _schedule; }
        }

        /// <summary>
        /// Reschedules departure from the specified location.
        /// </summary>
        /// <param name="location">location from where the rescheduled departure happens.</param>
        /// <param name="newDepartureTime">new departure time</param>
        public virtual void departureRescheduled(Location location, DateTime newDepartureTime)
        {
            var carrierMovements = new List<CarrierMovement>();

            foreach(CarrierMovement carrierMovement in _schedule.carrierMovements())
            {
                if(carrierMovement.DepartureLocation.sameAs(location))
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


        public virtual Location arrivalLocationWhenDepartedFrom(Location departureLocation)
        {
            foreach(CarrierMovement carrierMovement in _schedule.carrierMovements())
            {
                if(carrierMovement.DepartureLocation.sameAs(departureLocation))
                {
                    return carrierMovement.ArrivalLocation;
                }
            }

            return Location.NONE;
        }

        public virtual IEnumerable<Location> Locations
        {
            get
            {
                var locations = _schedule.carrierMovements().Select(cm => cm.DepartureLocation).ToList();

                locations.Add(_schedule.carrierMovements().Last().ArrivalLocation);

                return locations.AsReadOnly();
            }
        }

        public override string ToString()
        {
            return _voyageNumber.stringValue();
        }

        internal Voyage()
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