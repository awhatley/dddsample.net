using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// The different routing statuses of a cargo.
    /// </summary>
    public class RoutingStatus : ValueObject<RoutingStatus>
    {
        public static readonly RoutingStatus
            NOT_ROUTED,
            ROUTED,
            MISROUTED;

        public static RoutingStatus derivedFrom(Itinerary itinerary, RouteSpecification routeSpecification)
        {
            if(itinerary == null)
            {
                return NOT_ROUTED;
            }
            else
            {
                if(routeSpecification.isSatisfiedBy(itinerary))
                {
                    return ROUTED;
                }
                else
                {
                    return MISROUTED;
                }
            }
        }

        public bool sameValueAs(RoutingStatus other)
        {
            return this.Equals(other);
        }
    }
}