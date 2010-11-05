using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Travel;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Shared
{
    /// <summary>
    /// A handling activity represents how and where a cargo can be handled,
    /// and can be used to express predictions about what is expected to
    /// happen to a cargo in the future.
    /// </summary>
    public class HandlingActivity : ValueObjectSupport<HandlingActivity>
    {
        /// <summary>
        /// Type of handling
        /// </summary>
        public virtual HandlingActivityType Type { get; private set; }

        /// <summary>
        /// Location
        /// </summary>
        public virtual Location Location { get; private set; }

        /// <summary>
        /// Voyage
        /// </summary>
        public virtual Voyage Voyage { get; private set; }

        public HandlingActivity(HandlingActivityType type, Location location)
        {
            Validate.notNull(location, "Location is required");

            Type = type;
            Location = location;
            Voyage = null;
        }

        public HandlingActivity(HandlingActivityType type, Location location, Voyage voyage)
        {
            Validate.notNull(location, "Location is required");
            Validate.notNull(voyage, "Voyage is required");

            Type = type;
            Location = location;
            Voyage = voyage;
        }

        /// <summary>
        /// Copies this activity.
        /// </summary>
        public virtual HandlingActivity Copy()
        {
            return new HandlingActivity(Type, Location, Voyage);
        }

        public override string ToString()
        {
            return Type + " in " + Location + (Voyage != null ? ", " + Voyage : "");
        }

        protected internal HandlingActivity()
        {
        }

        // DSL-like factory methods

        public static InLocation LoadOnto(Voyage voyage)
        {
            return new InLocation(HandlingActivityType.LOAD, voyage);
        }

        public static InLocation UnloadOff(Voyage voyage)
        {
            return new InLocation(HandlingActivityType.UNLOAD, voyage);
        }

        public static HandlingActivity ReceiveIn(Location location)
        {
            return new HandlingActivity(HandlingActivityType.RECEIVE, location);
        }

        public static HandlingActivity ClaimIn(Location location)
        {
            return new HandlingActivity(HandlingActivityType.CLAIM, location);
        }

        public static HandlingActivity CustomsIn(Location location)
        {
            return new HandlingActivity(HandlingActivityType.CUSTOMS, location);
        }

        public class InLocation
        {
            private readonly HandlingActivityType _type;
            private readonly Voyage _voyage;

            public InLocation(HandlingActivityType type, Voyage voyage)
            {
                _type = type;
                _voyage = voyage;
            }

            public HandlingActivity In(Location location)
            {
                return new HandlingActivity(_type, location, _voyage);
            }
        }
    }
}