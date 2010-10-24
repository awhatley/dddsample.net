﻿using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Shared
{
    /// <summary>
    /// A handling activity represents how and where a cargo can be handled,
    /// and can be used to express predictions about what is expected to
    /// happen to a cargo in the future.
    /// </summary>
    public class HandlingActivity : ValueObjectSupport<HandlingActivity>
    {
        private readonly HandlingActivityType _type;
        private readonly Location _location;
        private readonly Voyage _voyage;

        public HandlingActivity(HandlingActivityType type, Location location)
        {
            Validate.notNull(type, "Handling event type is required");
            Validate.notNull(location, "Location is required");

            this._type = type;
            this._location = location;
            this._voyage = null;
        }

        public HandlingActivity(HandlingActivityType type, Location location, Voyage voyage)
        {
            Validate.notNull(type, "Handling event type is required");
            Validate.notNull(location, "Location is required");
            Validate.notNull(voyage, "Voyage is required");

            this._type = type;
            this._location = location;
            this._voyage = voyage;
        }

        /// <summary>
        /// Type of handling
        /// </summary>
        /// <returns>Type of handling</returns>
        public HandlingActivityType type()
        {
            return _type;
        }

        /// <summary>
        /// Location
        /// </summary>
        /// <returns>Location</returns>
        public Location location()
        {
            return _location;
        }

        /// <summary>
        /// Voyage
        /// </summary>
        /// <returns>Voyage</returns>
        public Voyage voyage()
        {
            return _voyage;
        }

        /// <summary>
        /// Copies this activity.
        /// </summary>
        /// <returns>A copy of this activity</returns>
        public HandlingActivity copy()
        {
            return new HandlingActivity(_type, _location, _voyage);
        }

        public override string ToString()
        {
            return _type + " in " + _location + (_voyage != null ? ", " + _voyage : "");
        }

        HandlingActivity()
        {
            // Needed by Hibernate
            _type = null;
            _location = null;
            _voyage = null;
        }

        // DSL-like factory methods

        public static InLocation loadOnto(Voyage voyage)
        {
            return new InLocation(HandlingActivityType.LOAD, voyage);
        }

        public static InLocation unloadOff(Voyage voyage)
        {
            return new InLocation(HandlingActivityType.UNLOAD, voyage);
        }

        public static HandlingActivity receiveIn(Location location)
        {
            return new HandlingActivity(HandlingActivityType.RECEIVE, location);
        }

        public static HandlingActivity claimIn(Location location)
        {
            return new HandlingActivity(HandlingActivityType.CLAIM, location);
        }

        public static HandlingActivity customsIn(Location location)
        {
            return new HandlingActivity(HandlingActivityType.CUSTOMS, location);
        }

        public class InLocation
        {
            private readonly HandlingActivityType type;
            private readonly Voyage voyage;

            public InLocation(HandlingActivityType type, Voyage voyage)
            {
                this.type = type;
                this.voyage = voyage;
            }

            public HandlingActivity @in(Location location)
            {
                return new HandlingActivity(type, location, voyage);
            }
        }
    }
}