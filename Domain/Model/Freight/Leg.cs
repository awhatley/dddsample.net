using System;
using System.Collections.Generic;
using System.Linq;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// An itinerary consists of one or more legs.
    /// </summary>
    public class Leg : ValueObjectSupport<Leg>
    {
        public virtual Voyage Voyage { get; private set; }
        public virtual Location LoadLocation { get; private set; }
        public virtual Location UnloadLocation { get; private set; }
        public virtual DateTime LoadTime { get; private set; }
        public virtual DateTime UnloadTime { get; private set; }

        private Leg(Voyage voyage, Location loadLocation, Location unloadLocation, DateTime loadTime, DateTime unloadTime)
        {
            Validate.notNull(voyage, "Voyage is required");
            Validate.notNull(loadLocation, "Load location is required");
            Validate.notNull(unloadLocation, "Unload location is required");
            Validate.isTrue(loadTime != DateTime.MinValue, "Unload time is required");
            Validate.isTrue(unloadTime != DateTime.MinValue, "Unload time is required");
            Validate.isTrue(!loadLocation.sameAs(unloadLocation), "Load location can't be the same as unload location");
            Validate.isTrue(unloadTime > loadTime, "Load time cannot be before unload time");

            Voyage = voyage;
            LoadLocation = loadLocation;
            UnloadLocation = unloadLocation;
            LoadTime = loadTime;
            UnloadTime = unloadTime;
        }

        /// <summary>
        /// This simple factory takes the Leg's times from the state of the
        /// Voyage as of the time of construction.
        /// </summary>
        /// <remarks>
        /// A fuller version might also factor operational time
        /// in the port. For example, average unload time of the
        /// unloadLocation could be added to the eta of the vessel
        /// schedule, providing an estimated unload time.
        /// In a real system, the estimation of the unload time
        /// might be more complex.
        /// </remarks>
        /// <param name="voyage">voyage</param>
        /// <param name="loadLocation">load location</param>
        /// <param name="unloadLocation">unload location</param>
        /// <returns>A leg on this voyage between the given locations.</returns>
        public static Leg DeriveLeg(Voyage voyage, Location loadLocation, Location unloadLocation)
        {
            Validate.notNull(voyage, "Voyage is required");
            Validate.notNull(loadLocation, "Load location is required");
            Validate.notNull(unloadLocation, "Unload location is required");
            Validate.isTrue(voyage.Locations.Contains(loadLocation), "Load location must be part of the voyage");
            Validate.isTrue(voyage.Locations.Contains(unloadLocation), "Unload location must be part of the voyage");
            return new Leg(voyage, loadLocation, unloadLocation, voyage.Schedule.DepartureTimeAt(loadLocation),
                           voyage.Schedule.ArrivalTimeAt(unloadLocation));
        }

        /// <summary>
        /// Gets a new leg with the same load and unload locations, but with updated load/unload times.
        /// </summary>
        /// <param name="voyage">voyage</param>
        /// <returns>A new leg with the same load and unload locations, but with updated load/unload times.</returns>
        protected internal virtual Leg WithRescheduledVoyage(Voyage voyage)
        {
            return DeriveLeg(voyage, LoadLocation, UnloadLocation);
        }

        /// <summary>
        /// True if this legs matches the handling activity, i.e. the voyage and load location is the same in case of a load activity and so on.
        /// </summary>
        /// <param name="handlingActivity">handling activity</param>
        /// <returns>True if this legs matches the handling activity, i.e. the voyage and load location is the same in case of a load activity and so on.</returns>
        protected internal virtual bool MatchesActivity(HandlingActivity handlingActivity)
        {
            if(Voyage.sameAs(handlingActivity.Voyage))
            {
                if(handlingActivity.Type == HandlingActivityType.LOAD)
                {
                    return LoadLocation.sameAs(handlingActivity.Location);
                }
                if(handlingActivity.Type == HandlingActivityType.UNLOAD)
                {
                    return UnloadLocation.sameAs(handlingActivity.Location);
                }
            }

            return false;
        }

        protected internal virtual HandlingActivity DeriveLoadActivity()
        {
            return HandlingActivity.LoadOnto(Voyage).In(LoadLocation);
        }

        protected internal virtual HandlingActivity DeriveUnloadActivity()
        {
            return HandlingActivity.UnloadOff(Voyage).In(UnloadLocation);
        }

        public virtual IEnumerable<Location> IntermediateLocations
        {
            get
            {
                return Voyage.Locations
                    .SkipWhile(l => !LoadLocation.sameAs(l))
                    .Skip(1)
                    .TakeWhile(l => !UnloadLocation.sameAs(l))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public override string ToString()
        {
            return "Load in " + LoadLocation + " at " + LoadTime +
            " --- Unload in " + UnloadLocation + " at " + UnloadTime;
        }

        protected internal Leg()
        {
        }
    }
}