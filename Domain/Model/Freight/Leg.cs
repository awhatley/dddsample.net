using System;
using System.Collections.Generic;

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
            Validate.isTrue(!loadLocation.sameAs(unloadLocation), "Load location can't be the same as unload location");
            // TODO enable this
            //Validate.isTrue(unloadTime.after(loadTime));

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
            // TODO enable this (or perhaps the requirement for load/unlaod time covers this?)
            //voyage.locations().contains(loadLocation);
            //voyage.locations().contains(unloadLocation);
            Validate.notNull(voyage, "Voyage is required");
            return new Leg(voyage, loadLocation, unloadLocation, voyage.schedule().departureTimeAt(loadLocation),
                           voyage.schedule().arrivalTimeAt(unloadLocation));
        }

        public Voyage voyage()
        {
            return _voyage;
        }

        public Location loadLocation()
        {
            return _loadLocation;
        }

        public Location unloadLocation()
        {
            return _unloadLocation;
        }

        public DateTime loadTime()
        {
            return _loadTime;
        }

        public DateTime unloadTime()
        {
            return _unloadTime;
        }

        /// <summary>
        /// Gets a new leg with the same load and unload locations, but with updated load/unload times.
        /// </summary>
        /// <param name="voyage">voyage</param>
        /// <returns>A new leg with the same load and unload locations, but with updated load/unload times.</returns>
        internal Leg withRescheduledVoyage(Voyage voyage)
        {
            return Leg.deriveLeg(voyage, _loadLocation, _unloadLocation);
        }

        /// <summary>
        /// True if this legs matches the handling activity, i.e. the voyage and load location is the same in case of a load activity and so on.
        /// </summary>
        /// <param name="handlingActivity">handling activity</param>
        /// <returns>True if this legs matches the handling activity, i.e. the voyage and load location is the same in case of a load activity and so on.</returns>
        internal bool matchesActivity(HandlingActivity handlingActivity)
        {
            if(_voyage.sameAs(handlingActivity.voyage()))
            {
                if(handlingActivity.type() == HandlingActivityType.LOAD)
                {
                    return _loadLocation.sameAs(handlingActivity.location());
                }
                if(handlingActivity.type() == HandlingActivityType.UNLOAD)
                {
                    return _unloadLocation.sameAs(handlingActivity.location());
                }
            }

            return false;
        }

        internal HandlingActivity deriveLoadActivity()
        {
            return HandlingActivity.loadOnto(_voyage).@in(_loadLocation);
        }

        internal HandlingActivity deriveUnloadActivity()
        {
            return HandlingActivity.unloadOff(_voyage).@in(_unloadLocation);
        }

        public IEnumerable<Location> intermediateLocations()
        {
            var locations = new List<Location>();
            var it = _voyage.locations().GetEnumerator();

            it.MoveNext();
            var location = it.Current;
            for(; it.MoveNext() && !_loadLocation.sameAs(location); )
            {
            }

            it.MoveNext();
            location = it.Current;
            for(; it.MoveNext() && !_unloadLocation.sameAs(location); )
            {
                locations.Add(location);
                it.MoveNext();
                location = it.Current;
            }

            return locations.AsReadOnly();
        }

        public override string ToString()
        {
            return "Load in " + _loadLocation + " at " + _loadTime +
            " --- Unload in " + _unloadLocation + " at " + _unloadTime;
        }

        Leg()
        {
            // Needed by Hibernate
            _voyage = null;
            _loadLocation = _unloadLocation = null;
        }
    }
}