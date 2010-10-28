using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    /// <summary>
    /// The different routing statuses of a cargo.
    /// </summary>
    public enum RoutingStatus
    {
        NOT_ROUTED,
        ROUTED,
        MISROUTED,
    }
}