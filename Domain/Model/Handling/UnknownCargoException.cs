using System;

using DomainDrivenDelivery.Domain.Model.Freight;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Thrown when trying to register an event with an unknown tracking id.
    /// </summary>
    [Serializable]
    public sealed class UnknownCargoException : CannotCreateHandlingEventException
    {
        public UnknownCargoException(TrackingId trackingId)
            : base("No cargo with tracking id " + trackingId.Value + " exists in the system") { }
    }
}