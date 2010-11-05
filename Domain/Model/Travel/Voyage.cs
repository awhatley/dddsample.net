using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Patterns.Entity;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    public class Voyage : EntitySupport<Voyage, VoyageNumber>
    {
        /// <summary>
        /// Null object pattern
        /// </summary>
        public static readonly Voyage None = new Voyage(new VoyageNumber(""), Schedule.Empty);

        /// <summary>
        /// Voyage number.
        /// </summary>
        public virtual VoyageNumber VoyageNumber { get; private set; }

        /// <summary>
        /// Schedule.
        /// </summary>
        public virtual Schedule Schedule { get; private set; }

        public Voyage(VoyageNumber voyageNumber, Schedule schedule)
        {
            Validate.notNull(voyageNumber, "Voyage number is required");
            Validate.notNull(schedule, "Schedule is required");

            VoyageNumber = voyageNumber;
            Schedule = schedule;
        }

        public override VoyageNumber Identity
        {
            get { return VoyageNumber; }
        }

        /// <summary>
        /// Reschedules departure from the specified location.
        /// </summary>
        /// <param name="location">location from where the rescheduled departure happens.</param>
        /// <param name="newDepartureTime">new departure time</param>
        public virtual void DepartureRescheduled(Location location, DateTime newDepartureTime)
        {
            var carrierMovements = Schedule.CarrierMovements.Select(
                carrierMovement => carrierMovement.DepartureLocation.sameAs(location)
                                       ? carrierMovement.WithDepartureTime(newDepartureTime)
                                       : carrierMovement).ToList();

            Schedule = new Schedule(carrierMovements);
        }

        public virtual Location ArrivalLocationWhenDepartedFrom(Location departureLocation)
        {
            foreach(var carrierMovement in Schedule.CarrierMovements)
            {
                if(carrierMovement.DepartureLocation.sameAs(departureLocation))
                {
                    return carrierMovement.ArrivalLocation;
                }
            }

            return Location.None;
        }

        public virtual IEnumerable<Location> Locations
        {
            get
            {
                var locations = Schedule.CarrierMovements.Select(cm => cm.DepartureLocation).ToList();

                locations.Add(Schedule.CarrierMovements.Last().ArrivalLocation);

                return locations.AsReadOnly();
            }
        }

        public override string ToString()
        {
            return VoyageNumber.Value;
        }

        protected internal Voyage()
        {
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