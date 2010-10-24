using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Shared
{
    /// <summary>
    /// Handling activity type. May or may not be voyage related and may or may not be physical.
    /// </summary>
    public class HandlingActivityType : ValueObject<HandlingActivityType>
    {
        public static readonly HandlingActivityType
          LOAD = new HandlingActivityType(true, true),
          UNLOAD = new HandlingActivityType(true, true),
          RECEIVE = new HandlingActivityType(false, true),
          CLAIM = new HandlingActivityType(false, true),
          CUSTOMS = new HandlingActivityType(false, false);

        private readonly bool voyageRelated;
        private readonly bool physical;

        /// <summary>
        /// Private enum constructor.
        /// </summary>
        /// <param name="voyageRelated">whether or not a voyage is associated with this event type</param>
        /// <param name="physical">whether or not this event type is physical</param>
        private HandlingActivityType(bool voyageRelated, bool physical)
        {
            this.voyageRelated = voyageRelated;
            this.physical = physical;
        }

        /// <summary>
        /// True if a voyage association is required for this event type.
        /// </summary>
        /// <returns>True if a voyage association is required for this event type.</returns>
        public bool isVoyageRelated()
        {
            return voyageRelated;
        }

        /// <summary>
        /// True if this is a physical handling.
        /// </summary>
        /// <returns>True if this is a physical handling.</returns>
        public bool isPhysical()
        {
            return physical;
        }

        public bool sameValueAs(HandlingActivityType other)
        {
            return this.Equals(other);
        }
    }
}