using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Shared
{
    /// <summary>
    /// Handling activity type. May or may not be voyage related and may or may not be physical.
    /// </summary>
    public enum HandlingActivityType
    {
        LOAD,
        UNLOAD,
        RECEIVE,
        CLAIM,
        CUSTOMS,
    }

    // TODO: java-style enums?
    public static class HandlingActivityTypeExtensions
    {
        private static readonly Dictionary<HandlingActivityType, HandlingActivityTypeDescriptor> Map =
            new Dictionary<HandlingActivityType, HandlingActivityTypeDescriptor>
            {
                { HandlingActivityType.LOAD, new HandlingActivityTypeDescriptor(true, true) },
                { HandlingActivityType.UNLOAD, new HandlingActivityTypeDescriptor(true, true) },
                { HandlingActivityType.RECEIVE, new HandlingActivityTypeDescriptor(false, true) },
                { HandlingActivityType.CLAIM, new HandlingActivityTypeDescriptor(false, true) },
                { HandlingActivityType.CUSTOMS, new HandlingActivityTypeDescriptor(false, false) }
            };

        /// <summary>
        /// True if a voyage association is required for this event type.
        /// </summary>
        /// <returns>True if a voyage association is required for this event type.</returns>
        public static bool isVoyageRelated(this HandlingActivityType type)
        {
            return Map[type].isVoyageRelated();
        }

        /// <summary>
        /// True if this is a physical handling.
        /// </summary>
        /// <returns>True if this is a physical handling.</returns>
        public static bool isPhysical(this HandlingActivityType type)
        {
            return Map[type].isPhysical();
        }

        private class HandlingActivityTypeDescriptor : ValueObject<HandlingActivityType>
        {
            private readonly bool voyageRelated;
            private readonly bool physical;

            /// <summary>
            /// Private enum constructor.
            /// </summary>
            /// <param name="voyageRelated">whether or not a voyage is associated with this event type</param>
            /// <param name="physical">whether or not this event type is physical</param>
            public HandlingActivityTypeDescriptor(bool voyageRelated, bool physical)
            {
                this.voyageRelated = voyageRelated;
                this.physical = physical;
            }

            public bool isVoyageRelated()
            {
                return voyageRelated;
            }

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
}