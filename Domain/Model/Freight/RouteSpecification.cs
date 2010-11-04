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
        private readonly Location _origin;
        private readonly Location _destination;
        private readonly DateTime _arrivalDeadline;

        // Delegate specifications
        private readonly Specification<Itinerary> notNull;
        private readonly Specification<Itinerary> sameOrigin;
        private readonly Specification<Itinerary> sameDestination;
        private readonly Specification<Itinerary> meetsDeadline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteSpecification"/> class.
        /// </summary>
        /// <param name="origin">origin location - can't be the same as the destination</param>
        /// <param name="destination">destination location - can't be the same as the origin</param>
        /// <param name="arrivalDeadline">arrival deadline</param>
        public RouteSpecification(Location origin, Location destination, DateTime arrivalDeadline)
        {
            Validate.notNull(origin, "Origin is required");
            Validate.notNull(destination, "Destination is required");
            Validate.isTrue(!origin.sameAs(destination), "Origin and destination can't be the same: " + origin);

            this._origin = origin;
            this._destination = destination;
            this._arrivalDeadline = arrivalDeadline;
            this.notNull = new NotNullSpecification();
            this.sameOrigin = new SameOriginSpecification(this);
            this.sameDestination = new SameDestinationSpecification(this);
            this.meetsDeadline = new MeetsDeadlineSpecification(this);
        }

        /// <summary>
        /// Determines if the specified itinerary satisfies the routing specification.
        /// </summary>
        /// <param name="itinerary">itinerary</param>
        /// <returns>True if this route specification is satisfied by the itinerary,
        /// i.e. the cargo will be delivered according to requirements.</returns>
        public bool isSatisfiedBy(Itinerary itinerary)
        {
            return notNull.and(sameOrigin).and(sameDestination).and(meetsDeadline).isSatisfiedBy(itinerary);
        }

        /// <summary>
        /// Specified origin location.
        /// </summary>
        /// <returns>Specified origin location.</returns>
        public Location origin()
        {
            return _origin;
        }

        /// <summary>
        /// Specified destination location.
        /// </summary>
        /// <returns>Specified destination location.</returns>
        public Location destination()
        {
            return _destination;
        }

        /// <summary>
        /// Arrival deadline.
        /// </summary>
        /// <returns>Arrival deadline.</returns>
        public DateTime arrivalDeadline()
        {
            return _arrivalDeadline;
        }

        /// <summary>
        /// Gets a copy of this route specification but with new destination
        /// </summary>
        /// <param name="newDestination">destination of new route specification</param>
        /// <returns>A copy of this route specification but with new destination</returns>
        public RouteSpecification withDestination(Location newDestination)
        {
            return new RouteSpecification(_origin, newDestination, _arrivalDeadline);
        }

        /// <summary>
        /// Gets a copy of this route specification but with the new origin
        /// </summary>
        /// <param name="newOrigin">origin of new route specification</param>
        /// <returns>A copy of this route specification but with the new origin</returns>
        public RouteSpecification withOrigin(Location newOrigin)
        {
            return new RouteSpecification(newOrigin, _destination, _arrivalDeadline);
        }

        /// <summary>
        /// Gets a copy of this route specification but with the new arrival deadline
        /// </summary>
        /// <param name="newArrivalDeadline">arrival deadline of new route specification</param>
        /// <returns>A copy of this route specification but with the new arrival deadline</returns>
        public RouteSpecification withArrivalDeadline(DateTime newArrivalDeadline)
        {
            return new RouteSpecification(_origin, _destination, newArrivalDeadline);
        }

        public override string ToString()
        {
            return _origin + " - " + _destination + " by " + _arrivalDeadline;
        }

        RouteSpecification()
        {
            // Needed by Hibernate
            _origin = _destination = null;
            this.notNull = new NotNullSpecification();
            this.sameOrigin = new SameOriginSpecification(this);
            this.sameDestination = new SameDestinationSpecification(this);
            this.meetsDeadline = new MeetsDeadlineSpecification(this);
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
                return _parent._origin.sameAs(itinerary.initialLoadLocation());
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
                return _parent._destination.sameAs(itinerary.finalUnloadLocation());
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
                return _parent._arrivalDeadline > itinerary.finalUnloadTime();
            }
        }

        private abstract class FieldlessSpecification : AbstractSpecification<Itinerary>
        {
            public override bool Equals(Object that)
            {
                return that != null && this.GetType().Equals(that.GetType());
            }

            public override int GetHashCode()
            {
                return this.GetType().GetHashCode();
            }
        }
    }
}