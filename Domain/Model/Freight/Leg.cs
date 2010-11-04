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
        private readonly Voyage _voyage;
        private readonly Location _loadLocation;
        private readonly Location _unloadLocation;
        private readonly DateTime _loadTime;
        private readonly DateTime _unloadTime;

        // TODO hide this, use factory only
        public Leg(Voyage voyage, Location loadLocation, Location unloadLocation, DateTime loadTime, DateTime unloadTime)
        {
            Validate.notNull(voyage, "Voyage is required");
            Validate.notNull(loadLocation, "Load location is required");
            Validate.notNull(unloadLocation, "Unload location is required");
            Validate.isTrue(loadTime != DateTime.MinValue, "Unload time is required");
            Validate.isTrue(unloadTime != DateTime.MinValue, "Unload time is required");
            Validate.isTrue(!loadLocation.sameAs(unloadLocation), "Load location can't be the same as unload location");
            Validate.isTrue(unloadTime > loadTime, "Load time cannot be before unload time");

            this._voyage = voyage;
            this._loadLocation = loadLocation;
            this._unloadLocation = unloadLocation;
            this._loadTime = loadTime;
            this._unloadTime = unloadTime;
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
        public static Leg deriveLeg(Voyage voyage, Location loadLocation, Location unloadLocation)
        {
            Validate.notNull(voyage, "Voyage is required");
            Validate.notNull(loadLocation, "Load location is required");
            Validate.notNull(unloadLocation, "Unload location is required");
            Validate.isTrue(voyage.Locations.Contains(loadLocation), "Load location must be part of the voyage");
            Validate.isTrue(voyage.Locations.Contains(unloadLocation), "Unload location must be part of the voyage");
            return new Leg(voyage, loadLocation, unloadLocation, voyage.Schedule.departureTimeAt(loadLocation),
                           voyage.Schedule.arrivalTimeAt(unloadLocation));
        }

        public virtual Voyage Voyage
        {
            get { return _voyage; }
        }

        public virtual Location LoadLocation
        {
            get { return _loadLocation; }
        }

        public virtual Location UnloadLocation
        {
            get { return _unloadLocation; }
        }

        public virtual DateTime LoadTime
        {
            get { return _loadTime; }
        }

        public virtual DateTime UnloadTime
        {
            get { return _unloadTime; }
        }

        /// <summary>
        /// Gets a new leg with the same load and unload locations, but with updated load/unload times.
        /// </summary>
        /// <param name="voyage">voyage</param>
        /// <returns>A new leg with the same load and unload locations, but with updated load/unload times.</returns>
        protected internal virtual Leg withRescheduledVoyage(Voyage voyage)
        {
            return Leg.deriveLeg(voyage, _loadLocation, _unloadLocation);
        }

        /// <summary>
        /// True if this legs matches the handling activity, i.e. the voyage and load location is the same in case of a load activity and so on.
        /// </summary>
        /// <param name="handlingActivity">handling activity</param>
        /// <returns>True if this legs matches the handling activity, i.e. the voyage and load location is the same in case of a load activity and so on.</returns>
        protected internal virtual bool matchesActivity(HandlingActivity handlingActivity)
        {
            if(_voyage.sameAs(handlingActivity.Voyage))
            {
                if(handlingActivity.Type == HandlingActivityType.LOAD)
                {
                    return _loadLocation.sameAs(handlingActivity.Location);
                }
                if(handlingActivity.Type == HandlingActivityType.UNLOAD)
                {
                    return _unloadLocation.sameAs(handlingActivity.Location);
                }
            }

            return false;
        }

        protected internal virtual HandlingActivity deriveLoadActivity()
        {
            return HandlingActivity.loadOnto(_voyage).@in(_loadLocation);
        }

        protected internal virtual HandlingActivity deriveUnloadActivity()
        {
            return HandlingActivity.unloadOff(_voyage).@in(_unloadLocation);
        }

        public virtual IEnumerable<Location> IntermediateLocations
        {
            get
            {
                return _voyage.Locations
                    .SkipWhile(l => !_loadLocation.sameAs(l))
                    .Skip(1)
                    .TakeWhile(l => !_unloadLocation.sameAs(l))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public override string ToString()
        {
            return "Load in " + _loadLocation + " at " + _loadTime +
            " --- Unload in " + _unloadLocation + " at " + _unloadTime;
        }

        internal Leg()
        {
            // Needed by Hibernate
            _voyage = null;
            _loadLocation = _unloadLocation = null;
        }
    }
}