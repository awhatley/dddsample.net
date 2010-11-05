using System;

using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Patterns.Specification;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// Route specification. Describes where a cargo orign and destination is,
    /// and the arrival deadline.
    /// </summary>
    public class RouteSpecification : ValueObjectSupport<RouteSpecification>
    {
        // Delegate specifications
        private readonly Specification<Itinerary> _notNull;
        private readonly Specification<Itinerary> _sameOrigin;
        private readonly Specification<Itinerary> _sameDestination;
        private readonly Specification<Itinerary> _meetsDeadline;

        /// <summary>
        /// Specified origin location.
        /// </summary>
        public virtual Location Origin { get; private set; }

        /// <summary>
        /// Specified destination location.
        /// </summary>
        public virtual Location Destination { get; private set; }

        /// <summary>
        /// Arrival deadline.
        /// </summary>
        public virtual DateTime ArrivalDeadline { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteSpecification"/> class.
        /// </summary>
        /// <param name="origin">origin location - can't be the same as the destination</param>
        /// <param name="destination">destination location - can't be the same as the origin</param>
        /// <param name="arrivalDeadline">arrival deadline</param>
        public RouteSpecification(Location origin, Location destination, DateTime arrivalDeadline) : this()
        {
            Validate.notNull(origin, "Origin is required");
            Validate.notNull(destination, "Destination is required");
            Validate.isTrue(!origin.sameAs(destination), "Origin and destination can't be the same: " + origin);

            Origin = origin;
            Destination = destination;
            ArrivalDeadline = arrivalDeadline;
        }

        /// <summary>
        /// Determines if the specified itinerary satisfies the routing specification.
        /// </summary>
        /// <param name="itinerary">itinerary</param>
        /// <returns>True if this route specification is satisfied by the itinerary,
        /// i.e. the cargo will be delivered according to requirements.</returns>
        public virtual bool IsSatisfiedBy(Itinerary itinerary)
        {
            return _notNull.and(_sameOrigin).and(_sameDestination).and(_meetsDeadline).isSatisfiedBy(itinerary);
        }

        /// <summary>
        /// Gets a copy of this route specification but with new destination
        /// </summary>
        /// <param name="newDestination">destination of new route specification</param>
        /// <returns>A copy of this route specification but with new destination</returns>
        public virtual RouteSpecification WithDestination(Location newDestination)
        {
            return new RouteSpecification(Origin, newDestination, ArrivalDeadline);
        }

        /// <summary>
        /// Gets a copy of this route specification but with the new origin
        /// </summary>
        /// <param name="newOrigin">origin of new route specification</param>
        /// <returns>A copy of this route specification but with the new origin</returns>
        public virtual RouteSpecification WithOrigin(Location newOrigin)
        {
            return new RouteSpecification(newOrigin, Destination, ArrivalDeadline);
        }

        /// <summary>
        /// Gets a copy of this route specification but with the new arrival deadline
        /// </summary>
        /// <param name="newArrivalDeadline">arrival deadline of new route specification</param>
        /// <returns>A copy of this route specification but with the new arrival deadline</returns>
        public virtual RouteSpecification WithArrivalDeadline(DateTime newArrivalDeadline)
        {
            return new RouteSpecification(Origin, Destination, newArrivalDeadline);
        }

        /// <summary>
        /// Routing status.
        /// </summary>
        /// <param name="itinerary">itinerary</param>
        /// <returns>Routing status.</returns>
        public virtual RoutingStatus StatusOf(Itinerary itinerary)
        {
            if(itinerary == null)
                return RoutingStatus.NOT_ROUTED;

            return IsSatisfiedBy(itinerary) ? RoutingStatus.ROUTED : RoutingStatus.MISROUTED;
        }

        public override string ToString()
        {
            return Origin + " - " + Destination + " by " + ArrivalDeadline;
        }

        protected internal RouteSpecification()
        {
            _notNull = new NotNullSpecification();
            _sameOrigin = new SameOriginSpecification(this);
            _sameDestination = new SameDestinationSpecification(this);
            _meetsDeadline = new MeetsDeadlineSpecification(this);
        }

        // --- Private classes ---
        private sealed class NotNullSpecification : FieldlessSpecification
        {
            public override bool isSatisfiedBy(Itinerary itinerary)
            {
                return itinerary != null;
            }
        }

        private sealed class SameOriginSpecification : FieldlessSpecification
        {
            private readonly RouteSpecification _parent;

            public SameOriginSpecification(RouteSpecification parent)
            {
                _parent = parent;
            }

            public override bool isSatisfiedBy(Itinerary itinerary)
            {
                return _parent.Origin.sameAs(itinerary.InitialLoadLocation);
            }
        }

        private sealed class SameDestinationSpecification : FieldlessSpecification
        {
            private readonly RouteSpecification _parent;

            public SameDestinationSpecification(RouteSpecification parent)
            {
                _parent = parent;
            }

            public override bool isSatisfiedBy(Itinerary itinerary)
            {
                return _parent.Destination.sameAs(itinerary.FinalUnloadLocation);
            }
        }

        private sealed class MeetsDeadlineSpecification : FieldlessSpecification
        {
            private readonly RouteSpecification _parent;

            public MeetsDeadlineSpecification(RouteSpecification parent)
            {
                _parent = parent;
            }

            public override bool isSatisfiedBy(Itinerary itinerary)
            {
                return _parent.ArrivalDeadline > itinerary.FinalUnloadTime;
            }
        }

        private abstract class FieldlessSpecification : AbstractSpecification<Itinerary>
        {
            public override bool Equals(Object that)
            {
                return that != null && GetType().Equals(that.GetType());
            }

            public override int GetHashCode()
            {
                return GetType().GetHashCode();
            }
        }
    }
}